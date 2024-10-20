using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Windower.Api;

var config = await Config.Load(args.Single());

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var rest = new HttpClient();

app.MapPost("/api/gh", async (GitHubArtifacts request) =>
{
	if (request.Key != config.ApiKey) {
		return Results.StatusCode(403);
	}

	return Results.Ok();
});

app.Run();
