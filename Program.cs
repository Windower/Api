using System;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
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
app.MapPost("/api/gh", async (GitHubArtifacts request) => {
	if (request.Key != config.ApiKey) {
		return Results.StatusCode(403);
	}

	try {
		var handler = (UpdateHandler)(request.Repo switch {
			_ => throw new Exception($"Unhandled repository: {request.Repo}"),
		});

		using (await mutex.LockAsync()) {
			await handler.Initialize(config);

			using var responseStream = await rest.GetStreamAsync(request.Url);
			using var zip = new ZipArchive(responseStream, ZipArchiveMode.Read);

			foreach (var entry in zip.Entries) {
				using var entryStream = entry.Open();
				await handler.CheckVersion(entry.Name, entryStream);
			}

			await handler.Finalize(config);
		}

		return Results.Ok();
	} catch (Exception ex) {
		return Results.BadRequest(ex.Message);
	}
});

app.Run();
