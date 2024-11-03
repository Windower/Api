using System;

namespace Windower.Api;

public class HookUpdateHandler : SingleVersionedUpdateHandler {
	protected override String ElementName => "hook";
}
