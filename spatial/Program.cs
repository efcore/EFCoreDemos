using System;
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
            using (var context = new SensorContext())
            {
                context.SetupDatabase();
                Console.ReadLine();
            }

            using (var context = new SensorContext())
            {
                var currentLocation = new Point(0, 0);

                // Step 2: To use tag in follow query add following code after context.Measurements
                // .WithTag("This is my spatial query!")
                var nearestMesurements =
                    from m in context.Measurements
                    where m.Location.Distance(currentLocation) < 2.5
                    orderby m.Location.Distance(currentLocation) descending
                    select m;

                foreach (var m in nearestMesurements)
                {
                    Console.WriteLine($"A temperature of {m.Temperature} was detected on {m.Time} at {m.Location}.");
                }
            }
        }
    }

    public class SensorContext : DbContext
    {
        private static readonly ILoggerFactory _loggerFactory = new LoggerFactory()
            .AddConsole((s, l) => l == LogLevel.Information && s.EndsWith("Command"));

        public DbSet<Measurement> Measurements { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Step 1: To swith to Sqlite provider, remove call to UseSqlServer and add following
            // .UseSqlite("filename=demo.db", sqlOptions => sqlOptions.UseNetTopologySuite())
            optionsBuilder
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Demo.Spatial;Trusted_Connection=True;ConnectRetryCount=0",
                        sqlOptions => sqlOptions.UseNetTopologySuite())
                .UseLoggerFactory(_loggerFactory);
        }

        public void SetupDatabase()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
            AddRange(
              new Measurement { Time = DateTime.Now, Location = new Point(0, 0), Temperature = 0.0 },
              new Measurement { Time = DateTime.Now, Location = new Point(1, 1), Temperature = 0.1 },
              new Measurement { Time = DateTime.Now, Location = new Point(1, 2), Temperature = 0.2 },
              new Measurement { Time = DateTime.Now, Location = new Point(2, 1), Temperature = 0.3 },
              new Measurement { Time = DateTime.Now, Location = new Point(2, 2), Temperature = 0.4 });
            SaveChanges();
        }
    }

    public class Measurement
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public Point Location { get; set; }
        public double Temperature { get; set; }
    }
}
