using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Windower.Api;

public class PluginsUpdateHandler : VersionedUpdateHandler {
	protected override String RelativeTargetPath { get; } = "plugins";

	private String PluginsPath { get; set; } = null!;
	private XElement PluginsNode { get; set; } = null!;
	private IDictionary<String, Item> Plugins { get; set; } = new Dictionary<String, Item>();

	protected override void Initialize() {
		PluginsPath = Path.Combine(DevPath, "plugins");
		PluginsNode = Manifest.Root!.Element("plugins")!;
		Plugins = PluginsNode.Elements("plugin").ToDictionary(
			element => element.Element("name")!.Value.ToLowerInvariant(),
			Item.Parse
		);
	}

	protected override Item GetItem(String name) {
		var lower = name.ToLowerInvariant();
		if (!Plugins.TryGetValue(lower, out var item)) {
			var element = new XElement("plugin", new XElement("name", name), new XElement("version", new Version(0, 0, 0, 0)));
			item = Item.Parse(element);
			Plugins[lower] = item;

			Plugins
				.OrderBy(kvp => kvp.Key)
				.Last(kvp => kvp.Key == "luacore" || kvp.Key.CompareTo(lower) < 0)
				.Value.Element.AddAfterSelf(element);
		}

		return item;
	}
}
