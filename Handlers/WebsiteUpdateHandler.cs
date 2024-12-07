using System;

namespace Windower.Api.Handlers;

public class WebsiteUpdateHandler : RawUpdateHandler {
	protected override String GetRoot(Config config) =>
		config.WebsitePath;
}
