using System.Linq;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace Demos
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var context = new CampusContext())
            {
                context.SetupDatabase();
            }

            using (var context = new CampusContext())
            {

                var currentLocation = new Point(0, 0);

                var nearestBuildings =
                    from t in context.Buildings
                    // Query tag   
                    // .WithTag(@"This is my spatial query!")
                    where t.Location.Distance(currentLocation) < 2
                    select t;

                foreach (var building in nearestBuildings)
                {
                    System.Console.WriteLine($"Building {building.Name} is located in {building.Location}");
                }
            }

        }
    }

    public class CampusContext : DbContext
    {
        private static readonly ILoggerFactory _loggerFactory = new LoggerFactory()
            .AddConsole((s, l) => l == LogLevel.Information && s.EndsWith("Command"));


        public DbSet<Building> Buildings { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Demo.Spatial;Trusted_Connection=True;ConnectRetryCount=0",
                        sqlOptions => sqlOptions.UseNetTopologySuite())
                .UseLoggerFactory(_loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Change the underlying type
            // modelBuilder.Entity<Building>().Property(b => b.Location).HasColumnType("Geography");
        }

        public void SetupDatabase()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
            AddRange(
                new Building { Location = new Point(1, 1), Name = "35" },
                new Building { Location = new Point(1, 2), Name = "18" },
                new Building { Location = new Point(2, 1), Name = "24" },
                new Building { Location = new Point(2, 2), Name = "3" },
                new Building { Location = new Point(0, 0), Name = "44" });
            SaveChanges();
        }
    }

    public class Building
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Point Location { get; set; }
        
    }
}
