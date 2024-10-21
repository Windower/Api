using System;
using System.IO;
using System.Threading.Tasks;

namespace Windower.Api;

public abstract class UpdateHandler {
	public abstract Task Initialize(Config config);
	public abstract Task CheckVersion(String name, Stream stream);
	public abstract Task Finalize(Config config);
}
