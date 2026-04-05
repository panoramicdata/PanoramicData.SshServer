using System.ComponentModel;

namespace PanoramicData.SshServer;

/// <summary>
/// Specifies the reason for an SSH disconnect.
/// </summary>
public enum DisconnectReason
{
	/// <summary>
	/// No reason specified. Not used by the protocol.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	None = 0, // Not used by protocol

	/// <summary>
	/// The host is not allowed to connect.
	/// </summary>
	HostNotAllowedToConnect = 1,

	/// <summary>
	/// A protocol error occurred.
	/// </summary>
	ProtocolError = 2,

	/// <summary>
	/// Key exchange failed.
	/// </summary>
	KeyExchangeFailed = 3,

	/// <summary>
	/// Reserved.
	/// </summary>
	Reserved = 4,

	/// <summary>
	/// A MAC error occurred.
	/// </summary>
	MacError = 5,

	/// <summary>
	/// A compression error occurred.
	/// </summary>
	CompressionError = 6,

	/// <summary>
	/// The requested service is not available.
	/// </summary>
	ServiceNotAvailable = 7,

	/// <summary>
	/// The protocol version is not supported.
	/// </summary>
	ProtocolVersionNotSupported = 8,

	/// <summary>
	/// The host key is not verifiable.
	/// </summary>
	HostKeyNotVerifiable = 9,

	/// <summary>
	/// The connection was lost.
	/// </summary>
	ConnectionLost = 10,

	/// <summary>
	/// The disconnect was initiated by the application.
	/// </summary>
	ByApplication = 11,

	/// <summary>
	/// Too many connections.
	/// </summary>
	TooManyConnections = 12,

	/// <summary>
	/// Authentication was cancelled by the user.
	/// </summary>
	AuthCancelledByUser = 13,

	/// <summary>
	/// No more authentication methods are available.
	/// </summary>
	NoMoreAuthMethodsAvailable = 14,

	/// <summary>
	/// The user name is illegal.
	/// </summary>
	IllegalUserName = 15
}
