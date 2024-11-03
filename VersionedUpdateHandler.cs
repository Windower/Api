using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using PeNet;

namespace Windower.Api;

public abstract class VersionedUpdateHandler : UpdateHandler {
	protected String DevPath { get; set; } = null!;
	protected String ManifestPath { get; set; } = null!;
	protected String TargetPath { get; set; } = null!;
	protected XDocument Manifest { get; set; } = null!;
	protected Boolean Changed { get; set; }

	protected abstract String RelativeTargetPath { get; }

	public sealed override async Task Initialize(Config config) {
		DevPath = Path.Combine(config.FilesPath, "4", "dev");
		ManifestPath = Path.Combine(DevPath, "manifest.xml");
		TargetPath = Path.Combine(DevPath, RelativeTargetPath);
		using var stream = File.Open(ManifestPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		Manifest = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

		Initialize();
	}

	public sealed override async Task CheckVersion(String filename, MemoryStream stream) {
		var buffer = stream.ToArray();
		var peFile = new PeFile(buffer);
		var version = Version.Parse(peFile.Resources!.VsVersionInfo!.StringFileInfo.StringTable[0].FileVersion!.Replace(", ", "."));
		var name = filename[..^4];

		var item = GetItem(name);

		if (version < item.Version) {
			throw new Exception($"{name} version {version} less than previous version {item.Version}.");
		}

		if (version == item.Version) {
			return;
		}

		await SaveFile(stream, Path.Combine(TargetPath, filename));

		item.Element.SetElementValue("version", version);

		Changed = true;
	}

	public sealed override async Task Finalize() {
		if (!Changed) {
			return;
		}

		await SaveXml(Manifest, ManifestPath);
	}

	protected abstract void Initialize();
	protected abstract Item GetItem(String name);

	protected record struct Item(String? Name, XElement Element, Version Version) {
		public static Item Parse(XElement element) =>
			new(element.Element("name")?.Value, element, Version.Parse(element.Element("version")!.Value));
	}
}
