using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PanoramicData.SshServer.Interfaces;

/// <summary>
/// Manages SSH host keys.
/// </summary>
public interface IKeyManager
{
	/// <summary>
	/// Gets the host keys asynchronously.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A dictionary mapping key type to base64-encoded key data.</returns>
	public Task<Dictionary<string, string>> GetHostKeysAsync(CancellationToken cancellationToken);
}
