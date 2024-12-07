using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Windower.Api;

public class Config {
	public String ApiKey { get; set; } = null!;
	public String ApiPath { get; set; } = null!;
	public String FilesPath { get; set; } = null!;
	public String WebsitePath { get; set; } = null!;

	public static async ValueTask<Config> Load(String[] args) {
		if (args.Length < 1) {
			Console.WriteLine("No input file specified.");
			Environment.Exit(1);
			return default;
		}

		try {
			var path = args[0];
			await using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
			return JsonSerializer.Deserialize<Config>(stream, SerializerSettings)!;
		} catch (Exception e) {
			Console.WriteLine($"Invalid input file: " + e.Message);
			Console.WriteLine(e.StackTrace);
			Environment.Exit(1);
			throw;
		}
	}

	private static readonly JsonSerializerOptions SerializerSettings = new() {
		AllowTrailingCommas = true,
	};

	[JsonConstructor]
	private Config() { }
}
