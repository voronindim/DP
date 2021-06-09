﻿using System;
using EventsLogger;
using Microsoft.Extensions.Logging;

namespace EventsLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });
            var eventsLogger = new EventsLogger(loggerFactory.CreateLogger<EventsLogger>());
            eventsLogger.Run();
        }
    }
}