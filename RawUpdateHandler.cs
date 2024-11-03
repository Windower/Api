using System;
using System.IO;
using System.Threading.Tasks;

namespace Windower.Api;

public abstract class RawUpdateHandler : UpdateHandler {
	private String RootPath { get; set; } = null!;

	public override Task Initialize(Config config) {
		RootPath = GetRoot(config);
		return Task.CompletedTask;
	}

	public override async Task CheckVersion(String filename, MemoryStream stream) =>
		await SaveFile(stream, Path.Combine(RootPath, filename));

	public override Task Finalize() =>
		Task.CompletedTask;

	protected abstract String GetRoot(Config config);
}
