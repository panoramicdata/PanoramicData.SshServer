using PanoramicData.SshServer.Messages.Connection;
using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace PanoramicData.SshServer.Services;

/// <summary>
/// Represents an abstract SSH channel.
/// </summary>
public abstract class Channel : IDisposable
{
	/// <summary>
	/// The connection service that owns this channel.
	/// </summary>
	protected ConnectionService _connectionService;

	/// <summary>
	/// Wait handle used to throttle sending when the window is exhausted.
	/// </summary>
	protected EventWaitHandle _sendingWindowWaitHandle = new ManualResetEvent(false);
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="Channel"/> class.
	/// </summary>
	/// <param name="connectionService">The connection service.</param>
	/// <param name="clientChannelId">The client channel identifier.</param>
	/// <param name="clientInitialWindowSize">The client initial window size.</param>
	/// <param name="clientMaxPacketSize">The client maximum packet size.</param>
	/// <param name="serverChannelId">The server channel identifier.</param>
	protected Channel(ConnectionService connectionService,
		uint clientChannelId, uint clientInitialWindowSize, uint clientMaxPacketSize,
		uint serverChannelId)
	{
		ArgumentNullException.ThrowIfNull(connectionService);

		_connectionService = connectionService;

		ClientChannelId = clientChannelId;
		ClientInitialWindowSize = clientInitialWindowSize;
		ClientWindowSize = clientInitialWindowSize;
		ClientMaxPacketSize = clientMaxPacketSize;

		ServerChannelId = serverChannelId;
		ServerInitialWindowSize = Session.InitialLocalWindowSize;
		ServerWindowSize = Session.InitialLocalWindowSize;
		ServerMaxPacketSize = Session.LocalChannelDataPacketSize;
	}

	/// <summary>
	/// Gets the client channel identifier.
	/// </summary>
	public uint ClientChannelId { get; private set; }

	/// <summary>
	/// Gets the client initial window size.
	/// </summary>
	public uint ClientInitialWindowSize { get; private set; }

	/// <summary>
	/// Gets or sets the client window size.
	/// </summary>
	public uint ClientWindowSize { get; protected set; }

	/// <summary>
	/// Gets the client maximum packet size.
	/// </summary>
	public uint ClientMaxPacketSize { get; private set; }

	/// <summary>
	/// Gets the server channel identifier.
	/// </summary>
	public uint ServerChannelId { get; private set; }

	/// <summary>
	/// Gets the server initial window size.
	/// </summary>
	public uint ServerInitialWindowSize { get; private set; }

	/// <summary>
	/// Gets or sets the server window size.
	/// </summary>
	public uint ServerWindowSize { get; protected set; }

	/// <summary>
	/// Gets the server maximum packet size.
	/// </summary>
	public uint ServerMaxPacketSize { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the client has closed the channel.
	/// </summary>
	public bool ClientClosed { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the client has sent EOF.
	/// </summary>
	public bool ClientMarkedEof { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the server has closed the channel.
	/// </summary>
	public bool ServerClosed { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the server has sent EOF.
	/// </summary>
	public bool ServerMarkedEof { get; private set; }

	/// <summary>
	/// Occurs when data is received on the channel.
	/// </summary>
	public event EventHandler<byte[]>? DataReceived;

	/// <summary>
	/// Occurs when EOF is received on the channel.
	/// </summary>
	public event EventHandler? EofReceived;

	/// <summary>
	/// Occurs when the channel is closed by the client.
	/// </summary>
	public event EventHandler? CloseReceived;

	/// <summary>
	/// Sends data to the client.
	/// </summary>
	/// <param name="data">The data to send.</param>
	public void SendData(byte[] data)
	{
		ArgumentNullException.ThrowIfNull(data);

		if (data.Length == 0)
		{
			return;
		}

		var msg = new ChannelDataMessage
		{
			RecipientChannel = ClientChannelId
		};

		var total = (uint)data.Length;
		var offset = 0L;
		byte[]? buf = null;
		do
		{
			var packetSize = Math.Min(Math.Min(ClientWindowSize, ClientMaxPacketSize), total);
			if (packetSize == 0)
			{
				_sendingWindowWaitHandle.WaitOne();
				continue;
			}

			if (buf == null || packetSize != buf.Length)
			{
				buf = new byte[packetSize];
			}

			Array.Copy(data, offset, buf, 0, packetSize);

			msg.Data = buf;
			_connectionService._session.SendMessage(msg);

			ClientWindowSize -= packetSize;
			total -= packetSize;
			offset += packetSize;
		} while (total > 0);
	}

	/// <summary>
	/// Sends an EOF message to the client.
	/// </summary>
	public void SendEof()
	{
		if (ServerMarkedEof)
		{
			return;
		}

		ServerMarkedEof = true;
		var msg = new ChannelEofMessage { RecipientChannel = ClientChannelId };
		_connectionService._session.SendMessage(msg);
	}

	/// <summary>
	/// Sends a close message to the client.
	/// </summary>
	public void SendClose() => SendClose(null);

	/// <summary>
	/// Sends a close message to the client with an optional exit code.
	/// </summary>
	/// <param name="exitCode">The optional exit code.</param>
	public void SendClose(uint? exitCode)
	{
		if (ServerClosed)
		{
			return;
		}

		ServerClosed = true;
		if (exitCode.HasValue)
		{
			_connectionService._session.SendMessage(new ExitStatusMessage { RecipientChannel = ClientChannelId, ExitStatus = exitCode.Value });
		}

		_connectionService._session.SendMessage(new ChannelCloseMessage { RecipientChannel = ClientChannelId });

		CheckBothClosed();
	}

	internal void OnData(byte[] data)
	{
		ArgumentNullException.ThrowIfNull(data);

		ServerAttemptAdjustWindow((uint)data.Length);

		DataReceived?.Invoke(this, data);
	}

	internal void OnEof()
	{
		ClientMarkedEof = true;

		EofReceived?.Invoke(this, EventArgs.Empty);
	}

	internal void OnClose()
	{
		ClientClosed = true;

		CloseReceived?.Invoke(this, EventArgs.Empty);

		CheckBothClosed();
	}

	internal void ClientAdjustWindow(uint bytesToAdd)
	{
		ClientWindowSize += bytesToAdd;

		// pulse multithreadings in same time and unsignal until thread switched
		// don't try to use AutoResetEvent
		_sendingWindowWaitHandle.Set();
		Thread.Sleep(1);
		_sendingWindowWaitHandle.Reset();
	}

	private void ServerAttemptAdjustWindow(uint messageLength)
	{
		ServerWindowSize -= messageLength;
		if (ServerWindowSize <= ServerMaxPacketSize)
		{
			_connectionService._session.SendMessage(new ChannelWindowAdjustMessage
			{
				RecipientChannel = ClientChannelId,
				BytesToAdd = ServerInitialWindowSize - ServerWindowSize
			});
			ServerWindowSize = ServerInitialWindowSize;
		}
	}

	private void CheckBothClosed()
	{
		if (ClientClosed && ServerClosed)
		{
			ForceClose();
		}
	}

	internal void ForceClose()
	{
		_connectionService.RemoveChannel(this);
		_sendingWindowWaitHandle.Set();
		_sendingWindowWaitHandle.Close();
	}

	/// <summary>
	/// Releases the unmanaged resources and optionally releases the managed resources.
	/// </summary>
	/// <param name="disposing">True to release both managed and unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_sendingWindowWaitHandle.Dispose();
			}

			_disposed = true;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
