using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TransportationExceptionManagement.Infrastructure.Persistence.Seeding;

namespace TransportationExceptionManagement.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<TransportationExceptionsDbContext>();
        await database.Database.MigrateAsync(cancellationToken);

        var seeder = scope.ServiceProvider.GetRequiredService<SyntheticDataSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
