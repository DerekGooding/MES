using Microsoft.EntityFrameworkCore;

namespace MES.Data;

public class DataContext : DbContext
{
    public DbSet<PartData> Parts { get; set; }
    public readonly string DbPath = "partsdata.db";


    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PartData>()
            .HasKey(p => p.PartId);
    }

}