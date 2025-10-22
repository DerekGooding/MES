using MES.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace MES.Data.Data;

public class DataContext : DbContext
{
    public DbSet<PartData> Parts { get; set; }
    private readonly string _connectionString;

    public DataContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)

            options.UseSqlite(_connectionString);

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<PartData>().HasKey(p => p.PartId);
}