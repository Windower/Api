using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Windower.Api.Handlers;

public class PluginsUpdateHandler : VersionedUpdateHandler {
	protected override String RelativeTargetPath { get; } = "plugins";

	private IDictionary<String, Item> Plugins { get; set; } = new Dictionary<String, Item>();

	protected override void Initialize() {
		Plugins = Manifest.Root!.Element("plugins")!.Elements("plugin").ToDictionary(
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
