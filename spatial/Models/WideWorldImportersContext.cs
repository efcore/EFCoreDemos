using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpatialDemo.Logging;
using SpatialDemo.Models.Configuration;

namespace SpatialDemo.Models
{
    class WideWorldImportersContext : DbContext
    {
        public static readonly LoggerFactory _loggerFactory = new LoggerFactory(new[] { new SqlLoggerProvider() });

        public DbSet<City> Cities { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Country> Countries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
                .UseSqlServer(
                    @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=WideWorldImporters",
                    x => x.UseNetTopologySuite())
                .UseLoggerFactory(_loggerFactory);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder
                .HasDefaultSchema("Application")
                .ApplyConfiguration(new CityConfiguration())
                .ApplyConfiguration(new StateConfiguration())
                .ApplyConfiguration(new CountryConfiguration());
    }
}
