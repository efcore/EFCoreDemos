// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Demos
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            using (var cosmosDb = new BloggingContext())
            {
                // Recreate database

                await cosmosDb.Database.EnsureDeletedAsync();
                await cosmosDb.Database.EnsureCreatedAsync();

                Console.WriteLine("Database created.");
                Console.ReadLine();

                // Add some data...
                cosmosDb.Blogs.AddRange(
                    new Blog
                    {
                        BlogId = 1,
                        Name = "ADO.NET",
                        Url = "http://blogs.msdn.com/adonet",
                        Posts = new List<Post>
                        {
                            new Post
                            {
                                PostId = 1,
                                Title = "Welcome to this blog!"
                            },
                            new Post
                            {
                                PostId = 2,
                                Title = "Getting Started with ADO.NET"
                            }
                        }
                    },
                    new Blog
                    {
                        BlogId = 2,
                        Name = "ASP.NET",
                        Url = "http://blogs.msdn.com/aspnet"
                    },
                    new Blog
                    {
                        BlogId = 3,
                        Name = ".NET",
                        Url = "http://blogs.msdn.com/dotnet"
                    });

                var affected = await cosmosDb.SaveChangesAsync();

                Console.WriteLine($"Saved {affected} records to Cosmos DB");
                Console.ReadLine();
            }

            using (var cosmosDb = new BloggingContext())
            {
                Console.WriteLine("Executing query for all blogs...");

                foreach (var blog in cosmosDb.Blogs)
                {
                    Console.WriteLine($"{blog.Name} - {blog.Url}");
                }

                Console.ReadLine();
                Console.WriteLine("Loading posts for ADO.NET blog...");

                var blog1 = cosmosDb.Blogs.Single(b => b.Name == "ADO.NET");

                cosmosDb.Entry(blog1).Collection(b => b.Posts).Load();

                foreach (var post in blog1.Posts)
                {
                    Console.WriteLine($" - {post.Title}");
                }

                Console.ReadLine();
                Console.Write("Modifying post of the blog...");

                blog1.Posts[0].Content = "Content Removed";

                var affected = await cosmosDb.SaveChangesAsync();

                Console.WriteLine($"Saved {affected} records to Cosmos DB");
                Console.ReadLine();
            }
        }
    }

    public class BloggingContext : DbContext
    {
        private static readonly ILoggerFactory loggerFactory = new LoggerFactory()
            .AddConsole((s, l) => l == LogLevel.Debug && s.EndsWith("Command"));

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseCosmosSql(
                    "https://localhost:8081",
                    "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                    "EFCoreDemo")
                .EnableSensitiveDataLogging()
                .UseLoggerFactory(loggerFactory);
        }

    }

    public class Blog
    {
        public int BlogId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public List<Post> Posts { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
