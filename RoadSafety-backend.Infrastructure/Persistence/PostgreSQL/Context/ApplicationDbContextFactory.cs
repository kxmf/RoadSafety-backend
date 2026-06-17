using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        LoadDotEnv();
        var connectionString = GetDefaultConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.UseNetTopologySuite());

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string GetDefaultConnectionString()
    {
        return Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
               ?? Environment.GetEnvironmentVariable("ConnectionStrings:DefaultConnection")
               ?? throw new InvalidOperationException(
                   "Default connection string is not configured. Set ConnectionStrings__DefaultConnection.");
    }

    private static void LoadDotEnv()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, ".env");
            if (File.Exists(path))
            {
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

                return;
            }

            directory = directory.Parent;
        }
    }
}
