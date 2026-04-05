using System.ComponentModel;

namespace PanoramicData.SshServer;

/// <summary>
/// Specifies the reason for a channel open failure.
/// </summary>
public enum ChannelOpenFailureReason
{
	/// <summary>
	/// No reason specified. Not used by the protocol.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	None = 0, // Not used by protocol

	/// <summary>
	/// The channel open was administratively prohibited.
	/// </summary>
	AdministrativelyProhibited = 1,

	/// <summary>
	/// The connection attempt failed.
	/// </summary>
	ConnectFailed = 2,

	/// <summary>
	/// The channel type is not recognized.
	/// </summary>
	UnknownChannelType = 3,

	/// <summary>
	/// There are insufficient resources available.
	/// </summary>
	ResourceShortage = 4,
}
