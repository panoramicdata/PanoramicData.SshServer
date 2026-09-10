using PanoramicData.SshServer.Messages;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace PanoramicData.SshServer;

/// <summary>
/// Framing, encrypting and authenticating the messages that cross the session.
/// </summary>
public partial class Session
{
	private Message ReceiveMessage()
	{
		var useAlg = _algorithms is not null;

		var blockSize = (byte)(useAlg ? Math.Max(8, _algorithms!.ClientEncryption.BlockBytesSize) : 8);
		var firstBlock = SocketRead(blockSize);
		if (useAlg)
		{
			firstBlock = _algorithms!.ClientEncryption.Transform(firstBlock);
		}

		var packetLength = firstBlock[0] << 24 | firstBlock[1] << 16 | firstBlock[2] << 8 | firstBlock[3];
		var paddingLength = firstBlock[4];
		var bytesToRead = packetLength - blockSize + 4;

		var followingBlocks = SocketRead(bytesToRead);
		if (useAlg)
		{
			followingBlocks = _algorithms!.ClientEncryption.Transform(followingBlocks);
		}

		var fullPacket = firstBlock.Concat(followingBlocks).ToArray();
		var data = fullPacket.Skip(5).Take(packetLength - paddingLength).ToArray();
		if (useAlg)
		{
			data = VerifyMacAndDecompress(fullPacket, data);
		}

		var message = CreateMessage(data);

		lock (_locker)
		{
			_inboundPacketSequence++;
			_inboundFlow += (uint)packetLength;
		}

		ConsiderReExchange();

		return message;
	}

	/// <summary>
	/// Checks the client MAC over the packet just read and decompresses its payload.
	/// </summary>
	/// <returns>The decompressed payload.</returns>
	/// <exception cref="SshConnectionException">The MAC does not match.</exception>
	private byte[] VerifyMacAndDecompress(byte[] fullPacket, byte[] data)
	{
		var clientMac = SocketRead(_algorithms!.ClientHmac.DigestLength);
		var mac = ComputeHmac(_algorithms.ClientHmac, fullPacket, _inboundPacketSequence);
		if (!clientMac.SequenceEqual(mac))
		{
			throw new SshConnectionException("Invalid MAC", DisconnectReason.MacError);
		}

		return _algorithms.ClientCompression.Decompress(data);
	}

	/// <summary>
	/// Builds the message the payload describes, or an <see cref="UnknownMessage"/> if this
	/// server does not implement that message number.
	/// </summary>
	private Message CreateMessage(byte[] data)
	{
		var typeNumber = data[0];
		if (!_messagesMetadata.TryGetValue(typeNumber, out var messageType))
		{
			return new UnknownMessage { SequenceNumber = _inboundPacketSequence, UnknownMessageType = typeNumber };
		}

		var message = Activator.CreateInstance(messageType) as Message
			?? throw new InvalidOperationException($"Could not create a message of type {messageType}.");
		message.Load(data);

		return message;
	}

	internal void SendMessage(Message message)
	{
		ArgumentNullException.ThrowIfNull(message);

		if (_exchangeContext != null
			&& message.MessageType > 4 && (message.MessageType < 20 || message.MessageType > 49))
		{
			_blockedMessages.Enqueue(message);
			return;
		}

		_hasBlockedMessagesWaitHandle.WaitOne();
		lock (_locker)
			SendMessageInternal(message);
	}

	private void SendMessageInternal(Message message)
	{
		var useAlg = _algorithms != null;

		var blockSize = (byte)(useAlg ? Math.Max(8, _algorithms!.ServerEncryption.BlockBytesSize) : 8);
		var payload = message.GetPacket();
		if (useAlg)
		{
			payload = _algorithms!.ServerCompression.Compress(payload);
		}

		// http://tools.ietf.org/html/rfc4253
		// 6.  Binary Packet Protocol
		// the total length of packet_length, padding_length, payload and padding
		// is a multiple of the cipher block size or 8,
		// padding length must between 4 and 255 bytes.
		var paddingLength = (byte)(blockSize - (payload.Length + 5) % blockSize);
		if (paddingLength < 4)
		{
			paddingLength += blockSize;
		}

		var packetLength = (uint)payload.Length + paddingLength + 1;

		var padding = new byte[paddingLength];
		RandomNumberGenerator.Fill(padding);

		using (var worker = new SshDataWorker())
		{
			worker.Write(packetLength);
			worker.Write(paddingLength);
			worker.Write(payload);
			worker.Write(padding);

			payload = worker.ToByteArray();
		}

		if (useAlg)
		{
			var mac = ComputeHmac(_algorithms!.ServerHmac, payload, _outboundPacketSequence);
			payload = [.. _algorithms!.ServerEncryption.Transform(payload), .. mac];
		}

		SocketWrite(payload);

		lock (_locker)
		{
			_outboundPacketSequence++;
			_outboundFlow += packetLength;
		}

		ConsiderReExchange();
	}

	private void ConsiderReExchange(bool force = false)
	{
		var kex = false;
		lock (_locker)
			if (_exchangeContext == null
				&& (force || _inboundFlow + _outboundFlow > 1024 * 1024 * 512)) // 0.5 GiB
			{
				_exchangeContext = new ExchangeContext();
				kex = true;
			}

		if (kex)
		{
			var kexInitMessage = LoadKexInitMessage();
			_exchangeContext!.ServerKexInitPayload = kexInitMessage.GetPacket();

			SendMessage(kexInitMessage);
		}
	}

	private void ContinueSendBlockedMessages()
	{
		if (!_blockedMessages.IsEmpty)
		{
			while (_blockedMessages.TryDequeue(out var message))
			{
				if (message is null)
				{
					continue;
				}

				SendMessageInternal(message);
			}
		}
	}

	internal bool TrySendMessage(Message message)
	{
		ArgumentNullException.ThrowIfNull(message);

		try
		{
			SendMessage(message);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static KeyExchangeInitMessage LoadKexInitMessage()
		=> new()
		{
			KeyExchangeAlgorithms = [.. _keyExchangeAlgorithms.Keys],
			ServerHostKeyAlgorithms = [.. _publicKeyAlgorithms.Keys],
			EncryptionAlgorithmsClientToServer = [.. _encryptionAlgorithms.Keys],
			EncryptionAlgorithmsServerToClient = [.. _encryptionAlgorithms.Keys],
			MacAlgorithmsClientToServer = [.. _hmacAlgorithms.Keys],
			MacAlgorithmsServerToClient = [.. _hmacAlgorithms.Keys],
			CompressionAlgorithmsClientToServer = [.. _compressionAlgorithms.Keys],
			CompressionAlgorithmsServerToClient = [.. _compressionAlgorithms.Keys],
			LanguagesClientToServer = [""],
			LanguagesServerToClient = [""],
			FirstKexPacketFollows = false,
			Reserved = 0
		};
}
