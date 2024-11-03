using System;

namespace Windower.Api.Handlers;

public class LauncherUpdateHandler : SingleVersionedUpdateHandler {
	protected override String ElementName => "launcher";
}
