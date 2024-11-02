using System;
using System.IO;
using System.Threading.Tasks;

namespace Windower.Api;

public class WebsiteUpdateHandler : UpdateHandler {
	private String RootPath { get; set; } = null!;

	public override Task Initialize(Config config) {
		RootPath = config.WebsitePath;
		return Task.CompletedTask;
	}

	public override async Task CheckVersion(String filename, MemoryStream stream) {
		var path = Path.Combine(RootPath, filename);

		await SaveFile(stream, path);
	}

	public override Task Finalize() =>
		Task.CompletedTask;
}
