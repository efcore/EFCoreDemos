using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MultiFeatureDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using (var context = new MyContext())
            {
                context.Database.EnsureDeleted();
                if (context.Database.EnsureCreated())
                {
                    context.AddRange(
                        new Blog
                        {
                            Name = ".NET Blog",
                            Url = "https://blogs.msdn.microsoft.com/dotnet",
                            Posts =
                            {
                                new Post
                                {
                                    Title = "Announcing Entity Framework Core 3.0",
                                    PublishDate = new DateTime(2019, 9, 12)
                                },
                                new Post
                                {
                                    Title = "Announcing Entity Framework Core 2.2",
                                    PublishDate = new DateTime(2018, 12, 4)
                                }
                            }
                        },
                        new Blog
                        {
                            Name = "Brice's Blog",
                            Url = "https://www.bricelam.net/",
                            Posts =
                            {
                                new Post
                                {
                                    Title = "Microsoft.Data.Sqlite 3.0",
                                    PublishDate = new DateTime(2019, 9, 18)
                                },
                                new Post
                                {
                                    Title = "Announcing Microsoft.Data.Sqlite 2.1",
                                    PublishDate = new DateTime(2018, 5, 24)
                                }
                            }
                        },
                        new Blog
                        {
                            Name = "One Unicorn...",
                            Posts =
                            {
                                new Post
                                {
                                    Title = "Magic Leap One is everything I hoped it would be",
                                    PublishDate = new DateTime(2019, 9, 4)
                                }
                            }
                        });

                    context.SaveChanges();
                }
            }

            using (var context = new MyContext(logCommand: true))
            {
                var startDate = new DateTime(2019, 1, 1);
                var endDate = new DateTime(2019, 12, 31);
                var blogsIn2019 = context.Blogs
                    .Where(b => b.Posts.Any(p => p.PublishDate >= startDate && p.PublishDate <= endDate));

                await foreach (var blog in blogsIn2019.AsAsyncEnumerable())
                {
                    Console.WriteLine(blog.Name);
                    Console.WriteLine(blog.Url);
                }

                Console.WriteLine();

                Console.WriteLine(blogsIn2019.ToQueryString());
            }

            Console.WriteLine("Program finished!");
        }
    }

    public class MyContext : DbContext
    {
        private readonly bool _logCommand;

        private static ILoggerFactory ContextLoggerFactory
            => new ConsoleLoggerFactory();

        public MyContext(bool logCommand = false)
        {
            _logCommand = logCommand;
        }

        // Declare DBSets
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Select 1 provider
            optionsBuilder
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=BloggingDemo;Trusted_Connection=True;Connect Timeout=5;ConnectRetryCount=0")
                //.UseSqlite("filename=BloggingDemo.db")
                //.UseCosmos("https://localhost:8081", @"C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", "BloggingDemo")
                .EnableSensitiveDataLogging();
            if (_logCommand)
            {
                optionsBuilder.UseLoggerFactory(ContextLoggerFactory);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure model
            modelBuilder.Entity<Tag>()
              .HasMany(e => e.Posts)
              .WithMany(e => e.Tags)
              .UsingEntity<PostTag>(
                pt => pt.HasOne<Post>().WithMany(),
                pt => pt.HasOne<Tag>().WithMany())
              .HasKey(e => new { e.PostId, e.TagId });
        }

        private class ConsoleLoggerFactory : ILoggerFactory
        {
            private readonly SqlLogger _logger;
            public ConsoleLoggerFactory()
            {
                _logger = new SqlLogger();
            }

            public void AddProvider(ILoggerProvider provider)
            {
            }

            public ILogger CreateLogger(string categoryName)
            {
                return _logger;
            }

            public void Dispose()
            {
            }

            private class SqlLogger : ILogger
            {
                public IDisposable BeginScope<TState>(TState state)
                {
                    return null;
                }

                public bool IsEnabled(LogLevel logLevel)
                {
                    return true;
                }

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                {
                    if (eventId == RelationalEventId.CommandExecuted)
                    {
                        var message = formatter(state, exception)?.Trim();
                        Console.WriteLine(message + Environment.NewLine);
                    }
                }
            }
        }
    }

    public class Blog
    {
        public int BlogId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public ICollection<Post> Posts { get; set; } = new HashSet<Post>();
    }

    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public DateTime PublishDate { get; set; }
        public Blog Blog { get; set; }
        public List<Tag> Tags { get; set; } // Skips over PostTag to Tag
    }

    public class Tag
    {
        public string TagId { get; set; }
        public string Content { get; set; }
        public List<Post> Posts { get; set; } // Skips over PostTag to Post
    }

    public class PostTag
    {
        public int PostId { get; set; }
        public string TagId { get; set; }

        public DateTime LinkCreated { get; set; }
    }
}
