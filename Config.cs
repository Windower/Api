using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Windower.Api;

public class Config {
	public String ApiKey { get; set; } = null!;
	public String FilesPath { get; set; } = null!;

	public static async Task<Config> Load(String path) {
		await using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
		return JsonSerializer.Deserialize<Config>(stream, SerializerSettings)!;
	}

	private static readonly JsonSerializerOptions SerializerSettings = new() {
		AllowTrailingCommas = true,
	};

	[JsonConstructor]
	private Config() { }
}
