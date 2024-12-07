using System;
using System.IO;
using System.Threading.Tasks;

namespace Windower.Api.Handlers;

public abstract class RawUpdateHandler : UpdateHandler {
	private String RootPath { get; set; } = null!;

	public override ValueTask Initialize(Config config) {
		RootPath = GetRoot(config);
		return ValueTask.CompletedTask;
	}

	public override async ValueTask ProcessFile(String filename, MemoryStream stream) =>
		await SaveFile(stream, Path.Combine(RootPath, filename));

	public override ValueTask Finalize() =>
		ValueTask.CompletedTask;

	protected abstract String GetRoot(Config config);
}
