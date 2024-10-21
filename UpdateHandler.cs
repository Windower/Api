using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Windower.Api;

public abstract class UpdateHandler {
	public abstract Task Initialize(Config config);
	public abstract Task CheckVersion(String filename, MemoryStream stream);
	public abstract Task Finalize();

	protected async Task SaveFile(MemoryStream stream, String path) {
		stream.Seek(0, SeekOrigin.Begin);
		using var file = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
		await stream.CopyToAsync(file);
	}

	protected async Task SaveXml(XDocument document, String path) {
		using var file = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
		using var writer = XmlWriter.Create(file, new() {
			Async = true,
			Encoding = new UTF8Encoding(false),
			OmitXmlDeclaration = true,
			Indent = true,
			NewLineChars = "\n",
			NewLineHandling = NewLineHandling.Replace,
		});
		await document.SaveAsync(writer, CancellationToken.None);
	}
}
