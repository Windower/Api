using System;

namespace Windower.Api;

public class GitHubActionRequest {
	public String ApiKey { get; set; } = null!;
	public Repository? Repository { get; set; }
	public String ArtifactUrl { get; set; } = null!;
	public String GitHubToken { get; set; } = null!;
}
