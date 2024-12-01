using System;
using System.IO;

namespace Windower.Api.Handlers;

public class BrandingUpdateHandler(String branch) : RawUpdateHandler {
	protected override String GetRoot(Config config) =>
		Path.Combine(config.ApiPath, "branding", branch);
}
