using System;

namespace Windower.Api;

public class GitHubArtifacts {
	public String Key { get; set; } = null!;
	public Repository? Repo { get; set; }
	public String Url { get; set; } = null!;
}
