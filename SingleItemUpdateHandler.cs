﻿using System;

namespace Windower.Api;

public abstract class SingleItemUpdateHandler : VersionedUpdateHandler {
	protected sealed override String RelativeTargetPath => String.Empty;

	protected abstract String ElementName { get; }

	private Item Single { get; set; }

	protected override void Initialize() {
		Single = Item.Parse(Manifest.Root!.Element(ElementName)!);
	}

	protected override Item GetItem(String name) =>
		Single;
}
