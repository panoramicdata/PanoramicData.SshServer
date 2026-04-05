using System.Text;

namespace PanoramicData.SshServer.Test;

public class SshDataWorkerTests
{
	[Fact]
	public void WriteBooleanTrueWritesOne()
	{
		using var worker = new SshDataWorker();
		worker.Write(true);

		var result = worker.ToByteArray();

		Assert.Single(result);
		Assert.Equal(1, result[0]);
	}

	[Fact]
	public void WriteBooleanFalseWritesZero()
	{
		using var worker = new SshDataWorker();
		worker.Write(false);

		var result = worker.ToByteArray();

		Assert.Single(result);
		Assert.Equal(0, result[0]);
	}

	[Fact]
	public void ReadBooleanTrueReturnsTrue()
	{
		using var worker = new SshDataWorker([1]);
		Assert.True(worker.ReadBoolean());
	}

	[Fact]
	public void ReadBooleanFalseReturnsFalse()
	{
		using var worker = new SshDataWorker([0]);
		Assert.False(worker.ReadBoolean());
	}

	[Fact]
	public void WriteByteRoundTrips()
	{
		using var writer = new SshDataWorker();
		writer.Write((byte)0xAB);

		using var reader = new SshDataWorker(writer.ToByteArray());
		Assert.Equal(0xAB, reader.ReadByte());
	}

	[Fact]
	public void WriteUInt32BigEndianRoundTrips()
	{
		using var writer = new SshDataWorker();
		writer.Write(0x01020304u);

		var bytes = writer.ToByteArray();

		Assert.Equal(4, bytes.Length);
		Assert.Equal(0x01, bytes[0]);
		Assert.Equal(0x02, bytes[1]);
		Assert.Equal(0x03, bytes[2]);
		Assert.Equal(0x04, bytes[3]);

		using var reader = new SshDataWorker(bytes);
		Assert.Equal(0x01020304u, reader.ReadUInt32());
	}

	[Fact]
	public void WriteUInt64BigEndianRoundTrips()
	{
		using var writer = new SshDataWorker();
		writer.Write(0x0102030405060708uL);

		var bytes = writer.ToByteArray();

		Assert.Equal(8, bytes.Length);

		using var reader = new SshDataWorker(bytes);
		Assert.Equal(0x0102030405060708uL, reader.ReadUInt64());
	}

	[Fact]
	public void WriteStringAsciiRoundTrips()
	{
		const string testString = "hello";

		using var writer = new SshDataWorker();
		writer.Write(testString, Encoding.ASCII);

		using var reader = new SshDataWorker(writer.ToByteArray());
		var result = reader.ReadString(Encoding.ASCII);

		Assert.Equal(testString, result);
	}

	[Fact]
	public void WriteStringUtf8RoundTrips()
	{
		const string testString = "héllo wörld";

		using var writer = new SshDataWorker();
		writer.Write(testString, Encoding.UTF8);

		using var reader = new SshDataWorker(writer.ToByteArray());
		var result = reader.ReadString(Encoding.UTF8);

		Assert.Equal(testString, result);
	}

	[Fact]
	public void WriteBinaryRoundTrips()
	{
		byte[] data = [0x01, 0x02, 0x03, 0x04, 0x05];

		using var writer = new SshDataWorker();
		writer.WriteBinary(data);

		using var reader = new SshDataWorker(writer.ToByteArray());
		var result = reader.ReadBinary();

		Assert.Equal(data, result);
	}

	[Fact]
	public void WriteBinaryWithOffsetAndCountRoundTrips()
	{
		byte[] data = [0x01, 0x02, 0x03, 0x04, 0x05];

		using var writer = new SshDataWorker();
		writer.WriteBinary(data, 1, 3);

		using var reader = new SshDataWorker(writer.ToByteArray());
		var result = reader.ReadBinary();

		Assert.Equal([0x02, 0x03, 0x04], result);
	}

	[Fact]
	public void WriteMpintZeroWritesZeroPadded()
	{
		using var writer = new SshDataWorker();
		writer.WriteMpint([0]);

		var bytes = writer.ToByteArray();

		// Should write 4 zero bytes (uint32 length of 0)
		Assert.Equal(4, bytes.Length);
		Assert.All(bytes, b => Assert.Equal(0, b));
	}

	[Fact]
	public void WriteMpintHighBitSetPrependsPaddingByte()
	{
		using var writer = new SshDataWorker();
		writer.WriteMpint([0x80, 0x01]);

		using var reader = new SshDataWorker(writer.ToByteArray());
		var length = reader.ReadUInt32();

		// Length should be 3 (padding byte + 2 data bytes)
		Assert.Equal(3u, length);
	}

	[Fact]
	public void WriteMpintNoHighBitNoPadding()
	{
		using var writer = new SshDataWorker();
		writer.WriteMpint([0x7F, 0x01]);

		using var reader = new SshDataWorker(writer.ToByteArray());
		var length = reader.ReadUInt32();

		Assert.Equal(2u, length);
	}

	[Fact]
	public void ReadMpintWithPaddingStripsPaddingByte()
	{
		using var writer = new SshDataWorker();
		writer.WriteMpint([0x80, 0x01]);

		using var reader = new SshDataWorker(writer.ToByteArray());
		var result = reader.ReadMpint();

		Assert.Equal([0x80, 0x01], result);
	}

	[Fact]
	public void ReadMpintEmptyReturnsSingleZero()
	{
		using var writer = new SshDataWorker();
		writer.Write(0u); // length = 0

		using var reader = new SshDataWorker(writer.ToByteArray());
		var result = reader.ReadMpint();

		Assert.Single(result);
		Assert.Equal(0, result[0]);
	}

	[Fact]
	public void DataAvailableReflectsPosition()
	{
		using var worker = new SshDataWorker([0x01, 0x02, 0x03, 0x04]);

		Assert.Equal(4, worker.DataAvailable);

		worker.ReadByte();
		Assert.Equal(3, worker.DataAvailable);

		worker.ReadByte();
		Assert.Equal(2, worker.DataAvailable);
	}

	[Fact]
	public void ReadBinaryInsufficientDataThrows()
	{
		using var worker = new SshDataWorker([0x01]);

		Assert.Throws<ArgumentOutOfRangeException>(() => worker.ReadBinary(2));
	}

	[Fact]
	public void WriteMultipleValuesReadInOrder()
	{
		using var writer = new SshDataWorker();
		writer.Write(true);
		writer.Write((byte)42);
		writer.Write(12345u);
		writer.Write("test", Encoding.ASCII);

		using var reader = new SshDataWorker(writer.ToByteArray());

		Assert.True(reader.ReadBoolean());
		Assert.Equal(42, reader.ReadByte());
		Assert.Equal(12345u, reader.ReadUInt32());
		Assert.Equal("test", reader.ReadString(Encoding.ASCII));
	}
}
