using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PanoramicData.SshServer;

public class SshServer(StartingInfo info) : IDisposable
{
	private readonly Lock _lock = new();
	private readonly List<Session> _sessions = [];
	private readonly Dictionary<string, string> _hostKey = [];
	private bool _started;
	private TcpListener _listener = null;
	private bool _disposedValue;

	public Guid Id { get; } = Guid.NewGuid();

	public StartingInfo StartingInfo { get; private set; } = info ?? throw new ArgumentNullException(nameof(info));

	public event EventHandler<Session> SessionStart;
	public event EventHandler<Session> SessionEnd;
	public event EventHandler<Exception> ExceptionRaised;

	public void Start()
	{
		lock (_lock)
		{
			if (_started)
			{
				throw new InvalidOperationException("The server is already started.");
			}

			_listener = StartingInfo.LocalAddress == IPAddress.IPv6Any
				? TcpListener.Create(StartingInfo.Port) // dual stack
				: new TcpListener(StartingInfo.LocalAddress, StartingInfo.Port);
			_listener.ExclusiveAddressUse = false;
			_listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			_listener.Start();
			BeginAcceptSocket();

			_started = true;
		}
	}

	public void Stop()
	{
		lock (_lock)
		{
			if (!_started)
			{
				return;
			}

			_listener.Stop();
			_started = false;

			foreach (var session in _sessions.ToArray())
			{
				try
				{
					session.Disconnect();
				}
				catch
				{
				}
			}
		}
	}

	public void AddHostKey(string type, string base64EncodedKey)
	{
		Contract.Requires(type != null);
		Contract.Requires(base64EncodedKey != null);
		_hostKey.TryAdd(type, base64EncodedKey);
	}

	private void BeginAcceptSocket()
	{
		try
		{
			_listener.BeginAcceptSocket(AcceptSocket, null);
		}
		catch (ObjectDisposedException)
		{
			return;
		}
		catch
		{
			if (_started)
				BeginAcceptSocket();
		}
	}

	private void AcceptSocket(IAsyncResult ar)
	{
		try
		{
			var socket = _listener.EndAcceptSocket(ar);
			Task.Run(() =>
			{
				var session = new Session(socket, _hostKey, StartingInfo.ServerBanner);

				session.Disconnected += (ss, ee) =>
				{
					lock (_lock)
					{
						SessionEnd?.Invoke(this, session);
						_sessions.Remove(session);
					}
				};

				lock (_lock)
				{
					_sessions.Add(session);
				}

				try
				{
					SessionStart?.Invoke(this, session);
					session.EstablishConnection();
				}
				catch (SshConnectionException ex)
				{
					session.Disconnect(ex.DisconnectReason, ex.Message);
					ExceptionRaised?.Invoke(this, ex);
				}
				catch (Exception ex)
				{
					session.Disconnect();
					ExceptionRaised?.Invoke(this, ex);
				}
			});
		}
		catch
		{
		}
		finally
		{
			BeginAcceptSocket();
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Stop();
				_listener.Dispose();
			}

			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
