// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Demos
{
    public class Program
    {
        private static void Main()
        {
            SetupDatabase();

            using (var db = new OrdersContext())
            {
                var orderSummaries = db.OrderSummaries;

                // Use query type with FromSql
                //var orderSummaries = orderSummaries.FromSql(
                //        @"SELECT o.Amount, p.Name AS ProductName, c.Name AS CustomerName
                //        FROM Orders o
                //        INNER JOIN Product p ON o.ProductId = p.Id
                //        INNER JOIN Customer c ON o.CustomerId = c.Id");

                foreach (var orderSummary in orderSummaries)
                {
                    Console.WriteLine(orderSummary);
                }
            }

            Console.Read();
        }

        private static void SetupDatabase()
        {
            using (var db = new OrdersContext())
            {
                db.Database.EnsureDeleted();

                if (db.Database.EnsureCreated())
                {
                    var diego = new Customer { Name = "Diego" };
                    var smit = new Customer { Name = "Smit" };

                    db.Set<Customer>().AddRange(diego, smit);

                    var empanada = new Product { Name = "Empanada" };
                    var applePie = new Product { Name = "Apple Pie" };

                    db.Set<Product>().AddRange(empanada, applePie);

                    db.Orders.Add(new Order { Amount = 12, Product = empanada, Customer = diego });
                    db.Orders.Add(new Order { Amount = 5, Product = applePie, Customer = smit });

                    db.SaveChanges();

                    // Map to database view
                    //db.Database.ExecuteSqlCommand(
                    //    @"CREATE VIEW [OrderSummaries] AS
                    //        SELECT o.Amount, p.Name AS ProductName, c.Name AS CustomerName
                    //        FROM Orders o
                    //        INNER JOIN Product p ON o.ProductId = p.Id
                    //        INNER JOIN Customer c ON o.CustomerId = c.Id");
                }
            }
        }

        public class OrdersContext : DbContext
        {
            private static readonly ILoggerFactory _loggerFactory = new LoggerFactory()
                .AddConsole((s, l) => l == LogLevel.Information && s.EndsWith("Command"));

            public DbSet<Order> Orders { get; set; }
            public DbQuery<OrderSummary> OrderSummaries { get; set; }


            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                // Map to SQL query
                //modelBuilder.Query<OrderSummary>().ToQuery(() => ...);
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Demo.ViewTypes;Trusted_Connection=True;ConnectRetryCount=0")
                    .UseLoggerFactory(_loggerFactory);
            }
        }

        public class Order
        {
            public int Id { get; set; }
            public int Amount { get; set; }
            public Product Product { get; set; }
            public Customer Customer { get; set; }
        }

        // Query type
        public class OrderSummary
        {
            public int Amount { get; set; }
            public string ProductName { get; set; }
            public string CustomerName { get; set; }

            public override string ToString()
            {
                return $"{CustomerName} ordered {Amount} of {ProductName}";
            }
        }

        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
