using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;
using Windower.Api;
using Windower.Api.Handlers;

var config = await Config.Load(args);

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSystemd();

builder.Services.Configure<JsonOptions>(options => {
	options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
	options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

var app = builder.Build();

var mutex = new AsyncLock();
var rest = new HttpClient();

app.MapGet("/ping", () => "pong");

app.MapPost("/gh", async (HttpRequest request) => {
	request.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>()!.MaxRequestBodySize = 100 * 1024 * 1024;

	try {
		var apiKey = GetField("api-key", "API key");
		if (apiKey != config.ApiKey) {
			return Results.StatusCode(403);
		}

		var repository = GetField("repository");
		var handler = (UpdateHandler)(repository switch {
			"Branding" => new BrandingUpdateHandler(GetField("branch")),
			"packages" => new PackagesUpdateHandler(),
			"Resources" => new ResourcesUpdateHandler(),
			"Plugins" => new PluginsUpdateHandler(),
			"Launcher" => new LauncherUpdateHandler(),
			"Windower4" => new HookUpdateHandler(),
			"windower_www" => new WebsiteUpdateHandler(),
			_ => throw new Exception($"Unhandled repository: {repository}"),
		});

		using (await mutex.LockAsync()) {
			await handler.Initialize(config);

			foreach (var entry in request.Form.Files) {
				using var entryStream = entry.OpenReadStream();
				using var memoryStream = new MemoryStream();
				await entryStream.CopyToAsync(memoryStream);
				await handler.ProcessFile(entry.Name, memoryStream);
			}

			await handler.Finalize();
		}

		return Results.Ok();

		String GetField(String key, String? identifier = null) =>
			request.Form[key].SingleOrDefault() ?? throw new Exception($"No {identifier ?? key} specified.");
	} catch (Exception ex) {
		return Results.BadRequest(ex.Message);
	}
});

app.Run();
