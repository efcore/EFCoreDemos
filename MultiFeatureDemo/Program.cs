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
                        new Author
                        {
                            Name = "Diego",
                            Blogs =
                            {
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
                                }
                            }
                        },
                        new Author
                        {
                            Name = "Brice",
                            Blogs =
                            {
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
                                }
                            }
                        },
                        new Author
                        {
                            Name = "Arthur",
                            Blogs =
                            {
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
                                }
                            }
                        });

                    context.SaveChanges();
                }
            }

            using (var context = new MyContext(logCommand: true))
            {
                var recentBlogs = context.Blogs
                    .Where(b => b.Posts.Any(p => EF.Functions.DateDiffDay(p.PublishDate, DateTime.Now) <= 7));
                var diegoBlogs = context.Blogs
                    .Where(b => b.Author.Name == "Diego");

                var query = recentBlogs.Union(diegoBlogs).TagWith("UnionQuery");
                query = query.OrderByDescending(p => p.Posts.Count).Skip(0).Take(2);

                var projectionQuery = query.Select(
                    b => new
                    {
                        b,
                        LatestPost = b.Posts.OrderByDescending(p => p.PublishDate).FirstOrDefault()
                    });

                await foreach (var blog in projectionQuery.AsAsyncEnumerable())
                {
                    Console.WriteLine(blog.b.Name);
                    Console.WriteLine($"  - {blog.LatestPost.Title}");
                }
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

            optionsBuilder.AddInterceptors(new MyCommandInterceptor());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure model
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

    public class MyCommandInterceptor : DbCommandInterceptor
    {
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            if (command.CommandText.Contains("UnionQuery"))
            {
                command.CommandText = command.CommandText + Environment.NewLine + "OPTION (MERGE UNION)";
            }

            return base.ReaderExecuting(command, eventData, result);
        }

        public override Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            if (command.CommandText.Contains("UnionQuery"))
            {
                command.CommandText = command.CommandText + Environment.NewLine + "OPTION (MERGE UNION)";
            }

            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }

    public class Author
    {
        public int AuthorId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public ICollection<Blog> Blogs { get; set; } = new HashSet<Blog>();
        public override string ToString() => $"{Name} {LastName}";
    }

    public class Blog
    {
        public int BlogId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public Author Author { get; set; }
        public ICollection<Post> Posts { get; set; } = new HashSet<Post>();
    }

    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public DateTime PublishDate { get; set; }
        public Blog Blog { get; set; }
    }
}
