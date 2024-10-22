using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using Windower.Api;

var config = await Config.Load(args.Single());

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options => {
	options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
	options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

var app = builder.Build();

var mutex = new AsyncLock();
var rest = new HttpClient();
app.MapPost("/api/gh", async (GitHubActionRequest action) => {
	if (action.ApiKey != config.ApiKey) {
		return Results.StatusCode(403);
	}

	try {
		var handler = (UpdateHandler)(action.Repository switch {
			Repository.Resources => new ResourcesUpdateHandler(),
			Repository.Plugins => new PluginsUpdateHandler(),
			Repository.Launcher => new LauncherUpdateHandler(),
			Repository.Windower4 => new HookUpdateHandler(),
			_ => throw new Exception($"Unhandled repository: {action.Repository}"),
		});

		Console.WriteLine($"Received ID: {action.ArtifactId}");
		var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/Windower/{action.Repository}/actions/artifacts{action.ArtifactId}/zip");
		request.Headers.Accept.Add(new("application/vnd.github+json"));
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", action.GitHubToken);
		request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
		using var response = await rest.SendAsync(request);
		var url = response.Headers.GetValues("Location").Single();
		Console.WriteLine($"Received URL: {url}");

		using var zipStream = await rest.GetStreamAsync(url);
		using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read);

		using (await mutex.LockAsync()) {
			await handler.Initialize(config);

			foreach (var entry in zip.Entries) {
				using var entryStream = entry.Open();
				using var memoryStream = new MemoryStream();
				await entryStream.CopyToAsync(memoryStream);
				await handler.CheckVersion(entry.Name, memoryStream);
			}

			await handler.Finalize();
		}

		return Results.Ok();
	} catch (Exception ex) {
		return Results.BadRequest(ex.Message);
	}
});

app.Run();
