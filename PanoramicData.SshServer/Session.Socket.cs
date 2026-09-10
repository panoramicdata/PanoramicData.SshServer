using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PanoramicData.SshServer;

/// <summary>
/// Reading from and writing to the session socket.
/// </summary>
public partial class Session
{
	private void SetSocketOptions()
	{
		const int socketBufferSize = 2 * MaximumSshPacketSize;
		_socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
		_socket.LingerState = new LingerOption(enable: false, seconds: 0);
		_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, socketBufferSize);
		_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, socketBufferSize);
		_socket.ReceiveTimeout = (int)_inactivityTimeout.TotalMilliseconds;
	}

	private string SocketReadProtocolVersion()
	{
		// http://tools.ietf.org/html/rfc4253#section-4.2
		var buffer = new byte[255];
		var dummy = new byte[255];
		var pos = 0;

		while (pos < buffer.Length)
		{
			var len = SocketPeek(buffer, pos);

			var version = TryTakeProtocolVersion(buffer, dummy, len, ref pos);
			if (version is not null)
			{
				return version;
			}

			_socket.Receive(dummy, 0, len, SocketFlags.None);
		}

		throw new SshConnectionException("Couldn't read the protocol version", DisconnectReason.ProtocolError);
	}

	/// <summary>
	/// Peeks at whatever the client has sent so far, without consuming it.
	/// </summary>
	/// <returns>The number of bytes now available in <paramref name="buffer"/> from <paramref name="pos"/>.</returns>
	private int SocketPeek(byte[] buffer, int pos)
	{
		var ar = _socket.BeginReceive(buffer, pos, buffer.Length - pos, SocketFlags.Peek, null, null);
		WaitHandle(ar);
		var len = _socket.EndReceive(ar);

		return len == 0
			? throw new SshConnectionException("Couldn't read the protocol version", DisconnectReason.ProtocolError)
			: len;
	}

	/// <summary>
	/// Looks for the end of the identification line in the peeked bytes and, if it is there,
	/// consumes the line and returns it.
	/// </summary>
	/// <returns>The identification string, or null if the line is not complete yet.</returns>
	private string? TryTakeProtocolVersion(byte[] buffer, byte[] dummy, int len, ref int pos)
	{
		for (var i = 0; i < len; i++, pos++)
		{
			if (pos > 0 && buffer[pos - 1] == CarriageReturn && buffer[pos] == LineFeed)
			{
				_socket.Receive(dummy, 0, i + 1, SocketFlags.None);
				return Encoding.ASCII.GetString(buffer, 0, pos - 1);
			}

			if (pos > 0 && buffer[pos] == LineFeed) // Non-RFC case
			{
				_socket.Receive(dummy, 0, i + 1, SocketFlags.None);
				return Encoding.ASCII.GetString(buffer, 0, pos);
			}
		}

		return null;
	}

	private void SocketWriteProtocolVersion() => SocketWrite(Encoding.ASCII.GetBytes(ServerVersion + "\r\n"));

	private byte[] SocketRead(int length)
	{
		var pos = 0;
		var buffer = new byte[length];

		var msSinceLastData = 0;

		while (pos < length)
		{
			try
			{
				var ar = _socket.BeginReceive(buffer, pos, length - pos, SocketFlags.None, null, null);
				WaitHandle(ar);
				var len = _socket.EndReceive(ar);
				if (!_socket.Connected)
				{
					throw new SshConnectionException("Connection lost", DisconnectReason.ConnectionLost);
				}

				if (len == 0 && _socket.Available == 0)
				{
					if (msSinceLastData >= _inactivityTimeout.TotalMilliseconds)
					{
						throw new SshConnectionException("Connection lost", DisconnectReason.ConnectionLost);
					}

					msSinceLastData += 50;
					Thread.Sleep(50);
				}
				else
				{
					msSinceLastData = 0;
				}

				pos += len;
			}
			catch (SocketException exp)
			{
				if (exp.SocketErrorCode is SocketError.WouldBlock or
					SocketError.IOPending or
					SocketError.NoBufferSpaceAvailable)
				{
					Thread.Sleep(30);
				}
				else
				{
					throw new SshConnectionException("Connection lost", DisconnectReason.ConnectionLost);
				}
			}
		}

		return buffer;
	}

	private void SocketWrite(byte[] data)
	{
		var pos = 0;
		var length = data.Length;

		while (pos < length)
		{
			try
			{
				var ar = _socket.BeginSend(data, pos, length - pos, SocketFlags.None, null, null);
				WaitHandle(ar);
				pos += _socket.EndSend(ar);
			}
			catch (SocketException ex)
			{
				if (ex.SocketErrorCode is SocketError.WouldBlock or
					SocketError.IOPending or
					SocketError.NoBufferSpaceAvailable)
				{
					Thread.Sleep(30);
				}
				else
				{
					throw new SshConnectionException("Connection lost", DisconnectReason.ConnectionLost);
				}
			}
		}
	}

	private void WaitHandle(IAsyncResult ar)
	{
		if (!ar.AsyncWaitHandle.WaitOne(_inactivityTimeout))
		{
			throw new SshConnectionException(string.Format("Socket operation has timed out after {0:F0} milliseconds.",
				_inactivityTimeout.TotalMilliseconds),
				DisconnectReason.ConnectionLost);
		}
	}
}
