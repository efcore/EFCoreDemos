using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Demos
{
    internal class Program
    {
        private static void Main()
        {
            SetupDatabase();

            using (var db = new BloggingContext())
            {
                var blog = new Blog { Name = "Rowan's Blog" };

                blog.SetUrl("http://romiller.com");

                db.Blogs.Add(blog);
                db.SaveChanges();
            }
            
            using (var db = new BloggingContext())
            {
                // Query the URL property
                
            }
        }

        private static void SetupDatabase()
        {
            using (var db = new BloggingContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }
        }
    }

    public class BloggingContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Demo.FlexibleMapping;Trusted_Connection=True;ConnectRetryCount=0;")
                .UseLoggerFactory(new LoggerFactory().AddConsole());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Set up a field mapping
            
        }
    }

    public class Blog
    {
        private string _url;

        public int BlogId { get; set; }
        public string Name { get; set; }

        public string GetUrl()
        {
            return _url;
        }

        public void SetUrl(string url)
        {
            // Perform some domain specific logic.

            _url = url;
        }
    }
}