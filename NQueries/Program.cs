using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace NQueries
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var context = new PersonContext())
            {
                // Recreate database

                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Add some data...
                context.Cities.AddRange(
                    new City { Name = "Seattle", Country = "USA" },
                    new City { Name = "Redmond", Country = "USA" },
                    new City { Name = "Paris", Country = "France" },
                    new City { Name = "Toronto", Country = "Canada" },
                    new City { Name = "Berlin", Country = "Germany" });

                context.People.AddRange(
                    new Person
                    {
                        FirstName = "Diego",
                        LastName = "Vega",
                        City = "Redmond"
                    },
                    new Person
                    {
                        FirstName = "Maurycy",
                        LastName = "Markowsky",
                        City = "Seattle"
                    },
                    new Person
                    {
                        FirstName = "Shay",
                        LastName = "Rojansky",
                        City = "Berlin"
                    },
                    new Person
                    {
                        FirstName = "Smit",
                        LastName = "Patel",
                        City = "Redmond"
                    });

                context.SaveChanges();
            }

            using (var context = new PersonContext())
            {
                var cities = (from c in context.Cities
                              where c.Country == "USA"
                              select c).ToList();

                var query = from p in context.People
                            where cities.Select(c => c.Name).Contains(p.City)
                            select new
                            {
                                p,
                                Fullname = p.FirstName + " " + p.LastName
                            };

                query.ToList();
            }
        }
    }

    public class PersonContext : DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<City> Cities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=_ModelApp;Trusted_Connection=True;Connect Timeout=5;ConnectRetryCount=0");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<City>().HasKey(e => new { e.Name, e.Country });
        }
    }

    public class Person
    {
        public int Id { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string FirstName { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string LastName { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string City { get; set; }
    }

    public class City
    {
        public string Name { get; set; }
        public string Country { get; set; }
    }
}
