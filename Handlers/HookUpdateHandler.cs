using System;

namespace Windower.Api.Handlers;

public class HookUpdateHandler : SingleVersionedUpdateHandler {
	protected override String ElementName => "hook";
}
