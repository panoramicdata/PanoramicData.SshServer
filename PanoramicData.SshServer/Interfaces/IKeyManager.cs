using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PanoramicData.SshServer.Interfaces;

public interface IKeyManager
{
	public Task<Dictionary<string, string>> GetHostKeysAsync(CancellationToken cancellationToken);
}
