// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Demos
{
    public class Program
    {
        public static void Main(string[] args)
        {

            using (var context = new BloggingContext())
            {
                // Recreate database

                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                Console.WriteLine("Database created.");
                Console.ReadLine();

                // Add some data...
                context.Blogs.AddRange(
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
                                Title = "Welcome to this blog!",
                                Tags = new List<Tag>
                                {
                                    new Tag
                                    {
                                        Name = "Meta"
                                    }
                                }

                            },
                            new Post
                            {
                                PostId = 2,
                                Title = "Getting Started with ADO.NET",
                                Tags = new List<Tag>
                                {
                                    new Tag
                                    {
                                        Name = "Entity Framework Core"
                                    },
                                    new Tag
                                    {
                                        Name = "ADO.NET"
                                    }
                                }
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

                var affected = context.SaveChanges();

                Console.WriteLine($"Saved {affected} records to Cosmos DB");
                Console.ReadLine();
            }

            using (var context = new BloggingContext())
            {
                Console.WriteLine("Executing query for all blogs...");

                foreach (var blog in context.Blogs)
                {
                    Console.WriteLine($"{blog.Name} - {blog.Url}");
                }

                Console.ReadLine();
                Console.WriteLine("Loading posts for ADO.NET blog...");

                var adonetBlog = context.Blogs.Single(b => b.Name == "ADO.NET");

                context.Entry(adonetBlog).Collection(b => b.Posts).Load();

                foreach (var post in adonetBlog.Posts)
                {
                    Console.WriteLine($" - {post.Title}");
                }

                Console.ReadLine();
                Console.Write("Modifying post of the blog...");

                adonetBlog.Posts[0].Content = "Content Removed";

                var affected = context.SaveChanges();

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
                .UseCosmos(
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
        public List<Tag> Tags { get; set; }
    }

    [Owned]
    public class Tag
    {
        [Key]
        public string Name { get; set; }
    }
}
