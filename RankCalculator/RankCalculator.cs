using NATS.Client;
using System;
using System.Text;
using Microsoft.Extensions.Logging;
using LibStorage;
using LoggingObjects;
using System.Threading.Tasks;
using System.Text.Json;

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

            _subscription = _conn.SubscribeAsync("valuator.processing.rank", "rank_calculator", async (sender, args) =>
            {
                string id = Encoding.UTF8.GetString(args.Message.Data);

                _logger.LogDebug("LOOKUP: {0}, {1}", id, storage.getSegmentId(id));

                var text = storage.value(id, "TEXT-" + id);
                string rankKey = "RANK-" + id;
                var rank = getRank(text);
                storage.store(id, rankKey, rank.ToString());

                await publishRankCalculatedEvent(id, rank);
            });
        }
        private async Task publishRankCalculatedEvent(string id, double rank)
        {
            Rank textRank = new Rank();
            textRank.textId = id;
            textRank.value = rank;

            ConnectionFactory cf = new ConnectionFactory();
            using (IConnection c = cf.CreateConnection())
            {
                byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(textRank));
                c.Publish("rank_calculator.rank_calculated", data);
                await Task.Delay(1000);

                c.Drain();
                c.Close();
            }
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