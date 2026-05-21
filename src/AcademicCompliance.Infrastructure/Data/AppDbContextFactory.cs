using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AcademicCompliance.Infrastructure.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=7799;Database=academic_compliance;Username=postgres;Password=7799";

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ACADEMIC_COMPLIANCE_CONNECTION_STRING")
            ?? DefaultConnectionString;

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
