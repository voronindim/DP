﻿using Valuator;
using Microsoft.Extensions.Logging;

namespace RankCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug)))
            {
                var storage = new StorageImpl(loggerFactory.CreateLogger<StorageImpl>());
                var rankCalculator = new RankCalculator(loggerFactory.CreateLogger<RankCalculator>(), storage);
                rankCalculator.Run();
            }
        }
    }
} 