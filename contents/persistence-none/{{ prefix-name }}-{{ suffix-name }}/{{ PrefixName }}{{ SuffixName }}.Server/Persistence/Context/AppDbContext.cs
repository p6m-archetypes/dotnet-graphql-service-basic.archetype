using Microsoft.EntityFrameworkCore;

namespace {{ PrefixName }}{{ SuffixName }}.Server.Persistence.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Add your DbSets here
    // Example:
    // public DbSet<{{ PrefixName }}> {{ PrefixName }}s { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure your entities here
    }
}
