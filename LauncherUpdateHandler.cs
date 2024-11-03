using System;

namespace Windower.Api;

public class LauncherUpdateHandler : SingleVersionedUpdateHandler {
	protected override String ElementName => "launcher";
}
