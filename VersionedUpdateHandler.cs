using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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
		Manifest = await ReadManifest();
		TargetPath = Path.Combine(DevPath, RelativeTargetPath);

		Initialize();
	}

	private async Task<XDocument> ReadManifest() {
		using var stream = File.Open(ManifestPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		return await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
	}

	public sealed override async Task CheckVersion(String filename, Stream stream) {
		using var memoryStream = new MemoryStream();
		await stream.CopyToAsync(memoryStream);
		var buffer = memoryStream.ToArray();
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

		using var file = File.Open(Path.Combine(TargetPath, item.Name ?? name), FileMode.Create, FileAccess.Write, FileShare.None);
		using var ms = new MemoryStream(buffer);
		await ms.CopyToAsync(file);

		item.Element.SetElementValue("version", version);

		Changed = true;
	}

	public sealed override async Task Finalize(Config config) {
		if (!Changed) {
			return;
		}

		using var file = File.Open(ManifestPath, FileMode.Create, FileAccess.Write, FileShare.None);
		using var writer = XmlWriter.Create(file, new() {
			Async = true,
			Encoding = new UTF8Encoding(false),
			OmitXmlDeclaration = true,
			Indent = true,
			NewLineChars = "\n",
			NewLineHandling = NewLineHandling.Entitize,
		});
		await Manifest.SaveAsync(writer, CancellationToken.None);
	}

	protected abstract void Initialize();
	protected abstract Item GetItem(String name);

	protected record struct Item(String? Name, XElement Element, Version Version) {
		public static Item Parse(XElement element) =>
			new(element.Element("name")?.Value, element, Version.Parse(element.Element("version")!.Value));
	}
}
