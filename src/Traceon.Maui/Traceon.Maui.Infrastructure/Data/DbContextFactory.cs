using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Arisoul.Traceon.Maui.Infrastructure.Data;

public class DbContextFactory : IDesignTimeDbContextFactory<TraceonDbContext>
{
    public TraceonDbContext CreateDbContext(string[] args = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TraceonDbContext>();
        optionsBuilder.UseSqlite("Data Source=traceon.db");
 
        return new TraceonDbContext(optionsBuilder.Options);
    }
}