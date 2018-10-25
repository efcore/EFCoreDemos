using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SpatialDemo.Logging
{
    class SqlLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
            => new SqlLogger(categoryName);

        public void Dispose()
        {
        }
    }

    class SqlLogger : ILogger
    {
        readonly string _categoryName;

        public SqlLogger(string categoryName)
            => _categoryName = categoryName;

        public IDisposable BeginScope<TState>(TState state)
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => _categoryName == DbLoggerCategory.Database.Command.Name
                && logLevel == LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (eventId != RelationalEventId.CommandExecuted)
                return;

            var stateList = (IReadOnlyList<KeyValuePair<string, object>>)state;

            var commandText = stateList
                .Where(p => p.Key == "commandText")
                .Select(p => p.Value)
                .OfType<string>()
                .FirstOrDefault();

            Console.WriteLine();

            using (var reader = new StringReader(commandText))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine("\t" + line);
                }
            }

            Console.WriteLine();
        }
    }
}
