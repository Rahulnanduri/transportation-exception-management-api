using Microsoft.EntityFrameworkCore;
using TransportationExceptionManagement.Domain.Entities;

namespace TransportationExceptionManagement.Infrastructure.Persistence;

public sealed class TransportationExceptionsDbContext(DbContextOptions<TransportationExceptionsDbContext> options)
    : DbContext(options)
{
    public DbSet<TransportationExceptionCase> Cases => Set<TransportationExceptionCase>();

    public DbSet<CaseNote> CaseNotes => Set<CaseNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransportationExceptionsDbContext).Assembly);
    }
}
