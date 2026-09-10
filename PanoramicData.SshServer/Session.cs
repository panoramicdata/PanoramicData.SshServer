using PanoramicData.SshServer.Algorithms;
using PanoramicData.SshServer.Messages;
using PanoramicData.SshServer.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace PanoramicData.SshServer;

/// <summary>
/// Represents an SSH session.
/// </summary>
public partial class Session
{
	private const byte CarriageReturn = 0x0d;
	private const byte LineFeed = 0x0a;
	internal const int MaximumSshPacketSize = LocalChannelDataPacketSize;
	internal const int InitialLocalWindowSize = LocalChannelDataPacketSize * 32;
	internal const int LocalChannelDataPacketSize = 1024 * 32;

	private static readonly Dictionary<byte, Type> _messagesMetadata;
	internal static readonly Dictionary<string, Func<KexAlgorithm>> _keyExchangeAlgorithms = [];
	internal static readonly Dictionary<string, Func<string?, PublicKeyAlgorithm>> _publicKeyAlgorithms = [];
	internal static readonly Dictionary<string, Func<CipherInfo>> _encryptionAlgorithms = [];
	internal static readonly Dictionary<string, Func<HmacInfo>> _hmacAlgorithms = [];
	internal static readonly Dictionary<string, Func<CompressionAlgorithm>> _compressionAlgorithms = [];

	private readonly ConcurrentDictionary<string, object?> _sessionVariables = [];

	private readonly Lock _locker = new();
	private readonly Socket _socket;
	private readonly TimeSpan _inactivityTimeout;
	private readonly Dictionary<string, string> _hostKey;

	private uint _outboundPacketSequence;
	private uint _inboundPacketSequence;
	private uint _outboundFlow;
	private uint _inboundFlow;
	private Algorithms? _algorithms;
	private ExchangeContext? _exchangeContext;
	private readonly List<SshService> _services = [];
	private readonly ConcurrentQueue<Message> _blockedMessages = new();
	private readonly ManualResetEvent _hasBlockedMessagesWaitHandle = new(true);

	/// <summary>
	/// Gets the server version string.
	/// </summary>
	public string ServerVersion { get; private set; }

	/// <summary>
	/// Gets the client version string.
	/// </summary>
	public string ClientVersion { get; private set; } = string.Empty;

	/// <summary>
	/// Gets the unique session identifier.
	/// </summary>
	public Guid Id { get; } = Guid.NewGuid();

	/// <summary>
	/// Gets the exchange hash.
	/// </summary>
	public byte[]? ExchangeHash { get; private set; }

	private readonly ConcurrentDictionary<uint, TerminalSize> _terminalSizes = new();

	/// <summary>
	/// Gets the terminal size for a specific server channel.
	/// </summary>
	/// <param name="serverChannelId">The server channel identifier.</param>
	/// <returns>The terminal size.</returns>
	public TerminalSize GetTerminalSize(uint serverChannelId)
	{
		if (!_terminalSizes.TryGetValue(serverChannelId, out var size))
		{
			_terminalSizes[serverChannelId] = size = new TerminalSize(80, 25, 640, 480);
		}

		return size;
	}

	/// <summary>
	/// Sets the terminal size for a specific server channel.
	/// </summary>
	/// <param name="serverChannelId">The server channel identifier.</param>
	/// <param name="size">The terminal size.</param>
	public void SetTerminalSize(uint serverChannelId, TerminalSize size)
		=> _terminalSizes[serverChannelId] = size;

	/// <summary>
	/// Gets a registered service of the specified type.
	/// </summary>
	/// <typeparam name="T">The service type.</typeparam>
	/// <returns>The service instance, or null if not registered.</returns>
	public T? GetService<T>() where T : SshService => (T?)_services.FirstOrDefault(x => x is T);

	static Session()
	{
		_keyExchangeAlgorithms.Add("diffie-hellman-group18-sha512", () => new DiffieHellmanGroupSha512(new DiffieHellman(8192)));
		_keyExchangeAlgorithms.Add("diffie-hellman-group16-sha512", () => new DiffieHellmanGroupSha512(new DiffieHellman(4096)));
		_keyExchangeAlgorithms.Add("diffie-hellman-group14-sha256", () => new DiffieHellmanGroupSha256(new DiffieHellman(2048)));
		_keyExchangeAlgorithms.Add("diffie-hellman-group14-sha1", () => new DiffieHellmanGroupSha1(new DiffieHellman(2048)));
		_keyExchangeAlgorithms.Add("diffie-hellman-group1-sha1", () => new DiffieHellmanGroupSha1(new DiffieHellman(1024)));

		_publicKeyAlgorithms.Add("rsa-sha2-256", x => new RsaKey(x));
		_publicKeyAlgorithms.Add("ssh-dss", x => new DssKey(x));

		_encryptionAlgorithms.Add("aes256-ctr", () => new CipherInfo(Aes.Create(), 256, CipherModeEx.CTR));
		_encryptionAlgorithms.Add("aes256-cbc", () => new CipherInfo(Aes.Create(), 256, CipherModeEx.CBC));

		_hmacAlgorithms.Add("hmac-sha2-256", () => new HmacInfo(new HMACSHA256(), 256));
		_hmacAlgorithms.Add("hmac-sha2-512", () => new HmacInfo(new HMACSHA512(), 512));

		_compressionAlgorithms.Add("none", () => new NoCompression());

		_messagesMetadata = (from t in typeof(Message).Assembly.GetTypes()
							 let attrib = (MessageAttribute?)t.GetCustomAttributes(typeof(MessageAttribute), false).FirstOrDefault()
							 where attrib != null
							 select new { attrib.Number, Type = t })
							 .ToDictionary(x => x.Number, x => x.Type);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Session"/> class.
	/// </summary>
	/// <param name="socket">The TCP socket.</param>
	/// <param name="hostKey">The host key dictionary.</param>
	/// <param name="serverBanner">The server banner string.</param>
	/// <param name="inactivityTimeout">The inactivity timeout.</param>
	public Session(Socket socket, Dictionary<string, string> hostKey, string serverBanner, TimeSpan inactivityTimeout)
	{
		ArgumentNullException.ThrowIfNull(socket);
		ArgumentNullException.ThrowIfNull(hostKey);
		ArgumentNullException.ThrowIfNull(serverBanner);

		_socket = socket;
		_hostKey = hostKey.ToDictionary(s => s.Key, s => s.Value);
		_inactivityTimeout = inactivityTimeout > TimeSpan.FromDays(365)
			? throw new ArgumentOutOfRangeException(nameof(inactivityTimeout), "Inactivity Timeout must be less than 1 year.")
			: inactivityTimeout;
		ServerVersion = serverBanner;
	}

	/// <summary>
	/// Occurs when the session is disconnected.
	/// </summary>
	public event EventHandler<EventArgs>? Disconnected;

	/// <summary>
	/// Occurs when a service is registered.
	/// </summary>
	public event EventHandler<SshService>? ServiceRegistered;

	/// <summary>
	/// Occurs when keys are exchanged.
	/// </summary>
	public event EventHandler<KeyExchangeArgs>? KeysExchanged;

	internal void EstablishConnection()
	{
		if (!_socket.Connected)
		{
			return;
		}

		SetSocketOptions();

		SocketWriteProtocolVersion();
		ClientVersion = SocketReadProtocolVersion();
		if (!SshVersionRegex().IsMatch(ClientVersion))
		{
			throw new SshConnectionException(
				string.Format("Not supported for client SSH version {0}. This server only supports SSH v2.0.", ClientVersion),
				DisconnectReason.ProtocolVersionNotSupported);
		}

		ConsiderReExchange(true);

		try
		{
			while (_socket != null && _socket.Connected)
			{
				var message = ReceiveMessage();
				if (message is UnknownMessage unknownMessage)
				{
					SendMessage(unknownMessage.MakeUnimplementedMessage());
				}
				else
				{
					HandleMessageCore(message);
				}
			}
		}
		finally
		{
			foreach (var service in _services)
			{
				service.CloseService();
			}
		}
	}

	/// <summary>
	/// Disconnects the session, reporting that the application closed it.
	/// </summary>
	public void Disconnect() => Disconnect(DisconnectReason.ByApplication);

	/// <summary>
	/// Disconnects the session for the given reason.
	/// </summary>
	/// <param name="reason">The disconnect reason.</param>
	public void Disconnect(DisconnectReason reason) => Disconnect(reason, "Connection terminated by the server.");

	/// <summary>
	/// Disconnects the session for the given reason, with a description.
	/// </summary>
	/// <param name="reason">The disconnect reason.</param>
	/// <param name="description">The disconnect description.</param>
	public void Disconnect(DisconnectReason reason, string description)
	{
		if (reason == DisconnectReason.ByApplication)
		{
			var message = new DisconnectMessage(reason, description);
			TrySendMessage(message);
		}

		try
		{
			_socket.Shutdown(SocketShutdown.Both);
			_socket.Close();
			_socket.Dispose();
		}
		catch (Exception)
		{
			// The socket is being torn down; whatever it reports at this point does not
			// change the outcome, and the Disconnected event below still has to fire.
		}

		Disconnected?.Invoke(this, EventArgs.Empty);
	}

	private sealed class Algorithms
	{
		public KexAlgorithm KeyExchange = null!;
		public PublicKeyAlgorithm PublicKey = null!;
		public EncryptionAlgorithm ClientEncryption = null!;
		public EncryptionAlgorithm ServerEncryption = null!;
		public HmacAlgorithm ClientHmac = null!;
		public HmacAlgorithm ServerHmac = null!;
		public CompressionAlgorithm ClientCompression = null!;
		public CompressionAlgorithm ServerCompression = null!;
	}

	private sealed class ExchangeContext
	{
		public string? KeyExchange;
		public string? PublicKey;
		public string? ClientEncryption;
		public string? ServerEncryption;
		public string? ClientHmac;
		public string? ServerHmac;
		public string? ClientCompression;
		public string? ServerCompression;

		public byte[]? ClientKexInitPayload;
		public byte[]? ServerKexInitPayload;

		public Algorithms? NewAlgorithms;
	}

	[GeneratedRegex("SSH-2.0-.+")]
	private static partial Regex SshVersionRegex();

	/// <summary>
	/// Tries to get a session variable by name.
	/// </summary>
	/// <typeparam name="T">The variable type.</typeparam>
	/// <param name="name">The variable name.</param>
	/// <param name="value">The variable value if found.</param>
	/// <returns>True if the variable exists and is of the correct type.</returns>
	public bool TryGetSessionVariable<T>(string name, out T value)
	{
		if (_sessionVariables.TryGetValue(name, out var obj) && obj is T t)
		{
			value = t;
			return true;
		}

		value = default!;

		return false;
	}

	/// <summary>
	/// Sets a session variable.
	/// </summary>
	/// <typeparam name="T">The variable type.</typeparam>
	/// <param name="name">The variable name.</param>
	/// <param name="value">The variable value.</param>
	public void SetSessionVariable<T>(string name, T value)
		=> _sessionVariables[name] = value;
}
