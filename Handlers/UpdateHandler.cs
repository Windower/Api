using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Windower.Api.Handlers;

public abstract class UpdateHandler {
	public abstract ValueTask Initialize(Config config);
	public abstract ValueTask ProcessFile(String filename, MemoryStream stream);
	public abstract ValueTask Finalize();

	protected async ValueTask SaveFile(MemoryStream stream, String path) {
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		stream.Seek(0, SeekOrigin.Begin);
		using var file = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
		await stream.CopyToAsync(file);
	}

	protected async ValueTask SaveXml(XDocument document, String path) {
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
