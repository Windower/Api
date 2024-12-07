using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Windower.Api.Handlers;

public class ResourcesUpdateHandler : UpdateHandler {
	private String RootPath { get; set; } = null!;
	private String ManifestPath { get; set; } = null!;
	private String ResourcesPath { get; set; } = null!;
	private ICollection<String> Files { get; set; } = [];

	public override ValueTask Initialize(Config config) {
		RootPath = Path.Combine(config.FilesPath, "res");
		ManifestPath = Path.Combine(RootPath, "manifest.xml");
		ResourcesPath = Path.Combine(RootPath, "lua");
		return ValueTask.CompletedTask;
	}

	public override async ValueTask ProcessFile(String filename, MemoryStream stream) {
		Files.Add(filename[..^4]);

		var buffer = stream.ToArray();
		var path = Path.Combine(ResourcesPath, filename);
		var file = File.Exists(path) ? await File.ReadAllBytesAsync(path) : [];
		if (buffer.SequenceEqual(file)) {
			return;
		}

		await SaveFile(stream, path);
	}

	public override async ValueTask Finalize() =>
		await SaveXml(new XDocument(new XElement("manifest", Files.Select(file => new XElement("file", file)))), ManifestPath);
}
