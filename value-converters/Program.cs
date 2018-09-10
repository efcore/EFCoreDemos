// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Drawing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Demos
{
    public class Program
    {
        private static void Main()
        {
            using (var db = new BloggingContext())
            {
                foreach (var theme in db.Themes)
                {
                    Console.WriteLine(
                        $"Id = {theme.ThemeId}, Name = {theme.Name}, Color = {theme.TitleColor}");
                }
            }

            Console.Read();
        }
    }

    public class Blog
    {
        public int BlogId { get; set; }
        public string BlogUrl { get; set; }
        public Theme Theme { get; set; }
    }

    public class Theme
    {
	  // built-in value converter
      public uint ThemeId { get; set; }
      public string Name { get; set; }
      public Color TitleColor { get; set; }
    }

    public class BloggingContext : DbContext
    {
        private static readonly ILoggerFactory _loggerFactory = new LoggerFactory()
            .AddConsole((s, l) => l == LogLevel.Information && s.EndsWith("Command"));

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Theme> Themes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Demo.ValueConverters;Trusted_Connection=True;ConnectRetryCount=0")
                .UseLoggerFactory(_loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed data
            modelBuilder
                .Entity<Theme>()
                .HasData(
                    new Theme { ThemeId = 1, Name = "MSDN", TitleColor = Color.Red },
                    new Theme { ThemeId = 2, Name = "TechNet", TitleColor = Color.DarkCyan },
                    new Theme { ThemeId = 3, Name = "Docs", TitleColor = Color.FloralWhite },
                    new Theme { ThemeId = 4, Name = "VS Developer Community", TitleColor = Color.LightBlue },
                    new Theme { ThemeId = 5, Name = "Personal", TitleColor = Color.LightGreen });

            // Custom value converter
            modelBuilder
              .Entity<Theme>()
              .Property(t => t.TitleColor)
              .HasConversion(c => c.Name, s => Color.FromName(s));

        }
    }
}