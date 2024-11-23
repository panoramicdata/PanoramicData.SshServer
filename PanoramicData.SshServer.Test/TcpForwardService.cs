using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ExampleApp;

public class TcpForwardService(string host, int port, string originatorIp, int originatorPort)
{
	private readonly Socket _socket = new(SocketType.Stream, ProtocolType.Tcp);
	private readonly List<byte> _blocked = [];
	private bool _connected = false;

	public event EventHandler<byte[]> DataReceived;
	public event EventHandler CloseReceived;

	public void Start() => Task.Run(() =>
	{
		try
		{
			MessageLoop();
		}
		catch
		{
			OnClose();
		}
	});

	public void OnData(byte[] data)
	{
		try
		{
			if (_connected)
			{
				if (_blocked.Count > 0)
				{
					_socket.Send(_blocked.ToArray());
					_blocked.Clear();
				}

				_socket.Send(data);
			}
			else
			{
				_blocked.AddRange(data);
			}
		}
		catch
		{
			OnClose();
		}
	}

	public void OnClose()
	{
		try
		{
			_socket.Shutdown(SocketShutdown.Send);
		}
		catch { }
	}

	private void MessageLoop()
	{
		_socket.Connect(host, port);
		_connected = true;
		OnData([]);
		var bytes = new byte[1024 * 64];
		while (true)
		{
			var len = _socket.Receive(bytes);
			if (len <= 0)
				break;

			var data = bytes.Length != len
				? bytes.Take(len).ToArray()
				: bytes;
			DataReceived?.Invoke(this, data);
		}

		CloseReceived?.Invoke(this, EventArgs.Empty);
	}
}
