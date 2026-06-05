using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var appSettingsPath = FindAppSettingsPath();
        var connectionString = GetDefaultConnectionString(appSettingsPath);

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.UseNetTopologySuite());

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string FindAppSettingsPath()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            var appSettingsPath = Path.Combine(directory.FullName, "RoadSafety-backend.Presentation", "appsettings.json");
            if (File.Exists(appSettingsPath))
                return appSettingsPath;

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find RoadSafety-backend.Presentation/appsettings.json.");
    }

    private static string GetDefaultConnectionString(string appSettingsPath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(appSettingsPath));

        return document.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("DefaultConnection")
            .GetString() ?? throw new InvalidOperationException("DefaultConnection is not configured.");
    }
}
