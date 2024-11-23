﻿using System.IO;

namespace PanoramicData.SshServer.Test;

public class GitService : CommandService
{
	public GitService(string command, string project)
		: base(Path.Combine(@"D:\PortableGit\mingw64\libexec\git-core", command + ".exe"),
			  Path.Combine(@"F:\Dev\GitTest\", project + ".git"))
	{
	}
}