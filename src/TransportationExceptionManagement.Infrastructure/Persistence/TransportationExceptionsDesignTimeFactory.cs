using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TransportationExceptionManagement.Infrastructure.Persistence;

public sealed class TransportationExceptionsDesignTimeFactory
    : IDesignTimeDbContextFactory<TransportationExceptionsDbContext>
{
    public TransportationExceptionsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TransportationExceptionsDbContext>()
            .UseSqlite("Data Source=transportation-exceptions.design.db")
            .Options;

        return new TransportationExceptionsDbContext(options);
    }
}
