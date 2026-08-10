using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TransportationExceptionManagement.Infrastructure.Persistence;
using TransportationExceptionManagement.Infrastructure.Persistence.Seeding;
using TransportationExceptionManagement.Tests.Support;

namespace TransportationExceptionManagement.Tests.Infrastructure;

public sealed class DatabaseInitializationTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    [Fact]
    public async Task Startup_AppliesInitialMigrationAndSeedsExactlyThirtySixCases()
    {
        _ = factory.CreateClient();
        await using var scope = factory.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<TransportationExceptionsDbContext>();

        var migrations = await database.Database.GetAppliedMigrationsAsync();
        var caseCount = await database.Cases.CountAsync();

        Assert.Contains(migrations, migration => migration.EndsWith("_InitialCreate", StringComparison.Ordinal));
        Assert.Equal(SyntheticDataSeeder.SeedCaseCount, caseCount);
        Assert.Equal(36, caseCount);
    }

    [Fact]
    public async Task Seeder_WhenDatabaseIsNotEmptyDoesNotDuplicateCases()
    {
        _ = factory.CreateClient();
        await using var scope = factory.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<TransportationExceptionsDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<SyntheticDataSeeder>();
        var before = await database.Cases.CountAsync();

        await seeder.SeedAsync();
        var after = await database.Cases.CountAsync();

        Assert.Equal(36, before);
        Assert.Equal(before, after);
    }
}
