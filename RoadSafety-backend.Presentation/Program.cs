using Microsoft.EntityFrameworkCore;
using RoadSafety_backend.Application;
using RoadSafety_backend.Infrastructure;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;
using RoadSafety_backend.Presentation;
using Scalar.AspNetCore;

LoadDotEnv();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApplication()
                .AddInfrastructure(builder.Configuration)
                .AddPresentation(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void LoadDotEnv()
{
    var path = FindDotEnvPath();
    if (!File.Exists(path))
        return;

    foreach (var line in File.ReadAllLines(path))
    {
        var trimmedLine = line.Trim();
        if (trimmedLine.Length == 0 || trimmedLine.StartsWith('#'))
            continue;

        var separatorIndex = trimmedLine.IndexOf('=');
        if (separatorIndex <= 0)
            continue;

        var key = trimmedLine[..separatorIndex].Trim();
        var value = trimmedLine[(separatorIndex + 1)..].Trim().Trim('"', '\'');

        if (Environment.GetEnvironmentVariable(key) is null)
            Environment.SetEnvironmentVariable(key, value);
    }
}

static string FindDotEnvPath()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

    while (directory is not null)
    {
        var path = Path.Combine(directory.FullName, ".env");
        if (File.Exists(path))
            return path;

        directory = directory.Parent;
    }

    return string.Empty;
}
