using System;
using System.IO;
using System.Text;

namespace PanoramicData.SshServer;

/// <summary>
/// Provides methods for reading and writing SSH protocol data.
/// </summary>
public class SshDataWorker : IDisposable
{
	private readonly MemoryStream _ms;
	private bool _disposedValue;

	/// <summary>
	/// Initializes a new instance of the <see cref="SshDataWorker"/> class.
	/// </summary>
	public SshDataWorker()
	{
		_ms = new MemoryStream(512);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SshDataWorker"/> class with the specified buffer.
	/// </summary>
	/// <param name="buffer">The data buffer to read from.</param>
	public SshDataWorker(byte[] buffer)
	{
		ArgumentNullException.ThrowIfNull(buffer);

		_ms = new MemoryStream(buffer);
	}

	/// <summary>
	/// Gets the number of bytes available to read.
	/// </summary>
	public long DataAvailable => _ms.Length - _ms.Position;

	/// <summary>
	/// Writes a boolean value.
	/// </summary>
	/// <param name="value">The boolean value.</param>
	public void Write(bool value) => _ms.WriteByte(value ? (byte)1 : (byte)0);

	/// <summary>
	/// Writes a byte value.
	/// </summary>
	/// <param name="value">The byte value.</param>
	public void Write(byte value) => _ms.WriteByte(value);

	/// <summary>
	/// Writes a 32-bit unsigned integer in big-endian format.
	/// </summary>
	/// <param name="value">The value to write.</param>
	public void Write(uint value)
	{
		var bytes = new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)(value & 0xFF) };
		_ms.Write(bytes, 0, 4);
	}

	/// <summary>
	/// Writes a 64-bit unsigned integer in big-endian format.
	/// </summary>
	/// <param name="value">The value to write.</param>
	public void Write(ulong value)
	{
		var bytes = new[] {
			(byte)(value >> 56), (byte)(value >> 48), (byte)(value >> 40), (byte)(value >> 32),
			(byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)(value & 0xFF)
		};
		_ms.Write(bytes, 0, 8);
	}

	/// <summary>
	/// Writes a string using the specified encoding.
	/// </summary>
	/// <param name="str">The string to write.</param>
	/// <param name="encoding">The character encoding.</param>
	public void Write(string str, Encoding encoding)
	{
		ArgumentNullException.ThrowIfNull(str);
		ArgumentNullException.ThrowIfNull(encoding);

		var bytes = encoding.GetBytes(str);
		WriteBinary(bytes);
	}

	/// <summary>
	/// Writes a multiple-precision integer.
	/// </summary>
	/// <param name="data">The integer data.</param>
	public void WriteMpint(byte[] data)
	{
		ArgumentNullException.ThrowIfNull(data);

		if (data.Length == 1 && data[0] == 0)
		{
			Write(new byte[4]);
		}
		else
		{
			var length = (uint)data.Length;
			var high = (data[0] & 0x80) != 0;
			if (high)
			{
				Write(length + 1);
				Write(0);
				Write(data);
			}
			else
			{
				Write(length);
				Write(data);
			}
		}
	}

	/// <summary>
	/// Writes a byte array.
	/// </summary>
	/// <param name="data">The data to write.</param>
	public void Write(byte[] data)
	{
		ArgumentNullException.ThrowIfNull(data);

		_ms.Write(data, 0, data.Length);
	}

	/// <summary>
	/// Writes a length-prefixed byte array.
	/// </summary>
	/// <param name="buffer">The data to write.</param>
	public void WriteBinary(byte[] buffer)
	{
		ArgumentNullException.ThrowIfNull(buffer);

		Write((uint)buffer.Length);
		_ms.Write(buffer, 0, buffer.Length);
	}

	/// <summary>
	/// Writes a length-prefixed portion of a byte array.
	/// </summary>
	/// <param name="buffer">The data buffer.</param>
	/// <param name="offset">The offset into the buffer.</param>
	/// <param name="count">The number of bytes to write.</param>
	public void WriteBinary(byte[] buffer, int offset, int count)
	{
		ArgumentNullException.ThrowIfNull(buffer);

		Write((uint)count);
		_ms.Write(buffer, offset, count);
	}

	/// <summary>
	/// Reads a boolean value.
	/// </summary>
	/// <returns>The boolean value.</returns>
	public bool ReadBoolean()
	{
		var num = _ms.ReadByte();

		if (num == -1)
			throw new EndOfStreamException();
		return num != 0;
	}

	/// <summary>
	/// Reads a single byte.
	/// </summary>
	/// <returns>The byte value.</returns>
	public byte ReadByte()
	{
		var data = ReadBinary(1);
		return data[0];
	}

	/// <summary>
	/// Reads a 32-bit unsigned integer in big-endian format.
	/// </summary>
	/// <returns>The value.</returns>
	public uint ReadUInt32()
	{
		var data = ReadBinary(4);
		return (uint)(data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]);
	}

	/// <summary>
	/// Reads a 64-bit unsigned integer in big-endian format.
	/// </summary>
	/// <returns>The value.</returns>
	public ulong ReadUInt64()
	{
		var data = ReadBinary(8);
		return (ulong)data[0] << 56 | (ulong)data[1] << 48 | (ulong)data[2] << 40 | (ulong)data[3] << 32 |
				(ulong)data[4] << 24 | (ulong)data[5] << 16 | (ulong)data[6] << 8 | data[7];
	}

	/// <summary>
	/// Reads a string using the specified encoding.
	/// </summary>
	/// <param name="encoding">The character encoding.</param>
	/// <returns>The decoded string.</returns>
	public string ReadString(Encoding encoding)
	{
		ArgumentNullException.ThrowIfNull(encoding);

		var bytes = ReadBinary();
		return encoding.GetString(bytes);
	}

	/// <summary>
	/// Reads a multiple-precision integer.
	/// </summary>
	/// <returns>The integer data.</returns>
	public byte[] ReadMpint()
	{
		var data = ReadBinary();

		if (data.Length == 0)
			return new byte[1];

		if (data[0] == 0)
		{
			var output = new byte[data.Length - 1];
			Array.Copy(data, 1, output, 0, output.Length);
			return output;
		}

		return data;
	}

	/// <summary>
	/// Reads a specified number of bytes.
	/// </summary>
	/// <param name="length">The number of bytes to read.</param>
	/// <returns>The byte array.</returns>
	public byte[] ReadBinary(int length)
	{
		var data = new byte[length];
		var bytesRead = _ms.Read(data, 0, length);

		return bytesRead < length ? throw new ArgumentOutOfRangeException(nameof(length)) : data;
	}

	/// <summary>
	/// Reads a length-prefixed byte array.
	/// </summary>
	/// <returns>The byte array.</returns>
	public byte[] ReadBinary()
	{
		var length = ReadUInt32();

		return ReadBinary((int)length);
	}

	/// <summary>
	/// Returns the contents as a byte array.
	/// </summary>
	/// <returns>The byte array.</returns>
	public byte[] ToByteArray() => _ms.ToArray();

	/// <summary>
	/// Releases the unmanaged resources and optionally releases the managed resources.
	/// </summary>
	/// <param name="disposing">True to release both managed and unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				_ms.Dispose();
			}

			_disposedValue = true;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		// Do not change this code. Put clean-up code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}