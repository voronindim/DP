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
        public async Task<IActionResult> OnPost(string text)
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            string similarityKey = "SIMILARITY-" + id;
            var similarityValue = similarity(text, id);
            _storage.store(similarityKey, similarityValue.ToString());

            publishSimilarityCalculatedEvent(id, similarityValue);
            
            string textKey = "TEXT-" + id;
            _storage.store(textKey, text);

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
            var pairs = _storage.values("TEXT-");

            foreach ( var pair in pairs) { 
                if (pair.Key != id && pair.Value == text) { 
                    return 1;
                }
            }

            return 0;
        }
    }
}
