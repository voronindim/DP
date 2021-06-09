using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NATS.Client;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibStorage;
using LoggingObjects;
using System.Text.Json;

namespace Valuator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private Storage _storage;
        public IndexModel(ILogger<IndexModel> logger, Storage storage)
        {
            _logger = logger;
            _storage = storage;
        }

        public void OnGet()
        {

        }
        public async Task<IActionResult> OnPost(string text, string country)
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            var segmentId = getSegmentIdByCountry(country);
            _logger.LogDebug("LOOKUP: {0}, {1}", id, segmentId);
            _storage.storeNewShardKey(id, segmentId);

            string similarityKey = "SIMILARITY-" + id;
            var similarityValue = similarity(text, id);
            _storage.store(id, similarityKey, similarityValue.ToString());

            publishSimilarityCalculatedEvent(id, similarityValue);
            
            string textKey = "TEXT-" + id;
            _storage.store(id, textKey, text);

            await createTaskForRankCalculator(id);
            
            return Redirect($"summary?id={id}");
        }

        private async Task createTaskForRankCalculator(string id)
        {
            CancellationTokenSource ct = new CancellationTokenSource();
            ConnectionFactory cf = new ConnectionFactory();

            using (IConnection c = cf.CreateConnection())
            {
                if (!ct.IsCancellationRequested)
                {
                    byte[] data = Encoding.UTF8.GetBytes(id);
                    c.Publish("valuator.processing.rank", data);
                    await Task.Delay(1000);
                }

                c.Drain();
                c.Close();
            }
        }

        private void publishSimilarityCalculatedEvent(string id, int similarity)
        {
            Similarity textSmilarity = new Similarity();
            textSmilarity.textId = id;
            textSmilarity.value = similarity;

            ConnectionFactory cf = new ConnectionFactory();
            using (IConnection c = cf.CreateConnection())
            {
                byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(textSmilarity));
                c.Publish("valuator.similarity_calculated", data);

                c.Drain();
                c.Close();
            }
        }

        private int similarity(String text, string id) { 
            id = "TEXT-" + id;
            return _storage.isTextExist(text) ? 1 : 0;
        }

        private string getSegmentIdByCountry(string country)
        {
            switch (country)
            {
                case "Russia":
                    return Constants.SEGMENT_ID_RUS;
                case "France":
                case "Germany":
                    return Constants.SEGMENT_ID_EU;
                case "USA":
                case "India":
                    return Constants.SEGMENT_ID_OTHER;
            }
            _logger.LogWarning("Country {0} doesn't support", country);
            return string.Empty;
        }
    }
}
