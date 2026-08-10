using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransportationExceptionManagement.Application.Abstractions;
using TransportationExceptionManagement.Infrastructure.Persistence;
using TransportationExceptionManagement.Infrastructure.Persistence.Repositories;
using TransportationExceptionManagement.Infrastructure.Persistence.Seeding;

namespace TransportationExceptionManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=transportation-exceptions.db";

        services.AddDbContext<TransportationExceptionsDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddScoped<ICaseRepository, CaseRepository>();
        services.AddScoped<SyntheticDataSeeder>();

        return services;
    }
}
