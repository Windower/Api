using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using PeNet;

namespace Windower.Api;

public class PluginsUpdateHandler : UpdateHandler {
	private String DevPath { get; set; } = null!;
	private String ManifestPath { get; set; } = null!;
	private String PluginsPath { get; set; } = null!;
	private XDocument Manifest { get; set; } = null!;
	private XElement PluginsNode { get; set; } = null!;
	private Boolean Changed { get; set; }
	private IDictionary<String, Plugin> Plugins { get; set; } = new Dictionary<String, Plugin>();

	public override async Task Initialize(Config config) {
		DevPath = Path.Combine(config.FilesPath, "4", "dev");
		ManifestPath = Path.Combine(DevPath, "manifest.xml");
		PluginsPath = Path.Combine(DevPath, "plugins");
		Manifest = await ReadManifest();
		PluginsNode = Manifest.Root!.Element("plugins")!;
		Plugins = PluginsNode.Elements("plugin").ToDictionary(
			element => element.Element("name")!.Value.ToLowerInvariant(),
			Plugin.Parse
		);
	}

	public override async Task CheckVersion(String filename, Stream stream) {
		using var memoryStream = new MemoryStream();
		await stream.CopyToAsync(memoryStream);
		var buffer = memoryStream.ToArray();
		var peFile = new PeFile(buffer);
		var version = Version.Parse(peFile.Resources!.VsVersionInfo!.StringFileInfo.StringTable[0].FileVersion!.Replace(", ", "."));

		var name = filename[..^4];
		var lower = name.ToLowerInvariant();
		if (!Plugins.TryGetValue(lower, out var plugin)) {
			var element = new XElement("plugin", new XElement("name", name), new XElement("version", new Version(0, 0, 0, 0)));
			plugin = Plugin.Parse(element);
			Plugins[lower] = plugin;

			Plugins
				.OrderBy(kvp => kvp.Key)
				.Last(kvp => kvp.Key == "luacore" || kvp.Key.CompareTo(lower) < 0)
				.Value.Element.AddAfterSelf(element);
		}

		if (version < plugin.Version) {
			throw new Exception($"{name} version {version} less than previous version {plugin.Version}.");
		}

		if (version == plugin.Version) {
			return;
		}

		using var file = File.Open(Path.Combine(PluginsPath, plugin.Name), FileMode.Create, FileAccess.Write, FileShare.None);
		using var ms = new MemoryStream(buffer);
		await ms.CopyToAsync(file);

		plugin.Element.SetElementValue("version", version);

		Changed = true;
	}

	public override async Task Finalize(Config config) {
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

	private async Task<XDocument> ReadManifest() {
		using var stream = File.Open(ManifestPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		return await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
	}

	private record struct Plugin(String Name, XElement Element, Version Version) {
		public static Plugin Parse(XElement element) =>
			new(element.Element("name")!.Value, element, Version.Parse(element.Element("version")!.Value));
	}
}
