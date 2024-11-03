using System;

namespace Windower.Api;

public class WebsiteUpdateHandler : RawUpdateHandler {
	protected override String GetRoot(Config config) =>
		config.WebsitePath;
}
