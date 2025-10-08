using MES.Common;
using Microsoft.EntityFrameworkCore;

namespace MES.Data;

public class DataContext : DbContext
{
    public DbSet<PartData> Parts { get; set; }
    public readonly string DbPath = "partsdata.db";

    public DataContext()
    {
    }
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            // Use SQLite database
            options.UseSqlite($"Data Source={DbPath}");

        }

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PartData>()
            .HasKey(p => p.PartId);
    }

}