using System;
using System.IO;

namespace Windower.Api.Handlers;

public class PackagesUpdateHandler : RawUpdateHandler {
	protected override String GetRoot(Config config) =>
		Path.Combine(config.FilesPath, "5", "packages");
}
