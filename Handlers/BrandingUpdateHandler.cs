using System;
using System.IO;

namespace Windower.Api.Handlers;

public class BrandingUpdateHandler : RawUpdateHandler {
	protected override String GetRoot(Config config) =>
		Path.Combine(config.ApiPath, "5", "branding");
}
