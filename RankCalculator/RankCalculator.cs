using NATS.Client;
using System;
using System.Text;
using Valuator;
using Microsoft.Extensions.Logging;

namespace RankCalculator
{
    public class RankCalculator
    {
        private IAsyncSubscription _subscription;
        private readonly ILogger<RankCalculator> _logger;
        IConnection _conn;
        public RankCalculator(ILogger<RankCalculator> logger, Storage storage)
        {
            _logger = logger;

            _conn = new ConnectionFactory().CreateConnection();

            _subscription = _conn.SubscribeAsync("valuator.processing.rank", "rank_calculator", (sender, args) =>
            {
                string id = Encoding.UTF8.GetString(args.Message.Data);
                var text = storage.value("TEXT-" + id);
                string rankKey = "RANK-" + id;
                var rank = getRank(text);
                storage.store(rankKey, rank.ToString());
            });
        }

        public void Run()
        {
            _subscription.Start();
            Console.ReadLine();        
        }

        private double getRank(string text)
        {
            var nonAlphabeticalCharsCounter = 0;
            foreach (var ch in text)
            {
                if (!Char.IsLetter(ch))
                {
                    nonAlphabeticalCharsCounter++;
                }
            }
            double rank =  Convert.ToDouble(nonAlphabeticalCharsCounter) / Convert.ToDouble(text.Length);
            _logger.LogDebug($"Text {text.Substring(0, Math.Min(10, text.Length))} (lenght {text.Length}) contains {nonAlphabeticalCharsCounter} non alphabetical chars and has rank {rank}");

            return rank;
        }

        ~RankCalculator()
        {
            _subscription.Unsubscribe();
            _conn.Drain();
            _conn.Close();  
        }

    }
} 