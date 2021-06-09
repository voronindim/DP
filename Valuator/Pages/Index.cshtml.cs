using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NATS.Client;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

            string textKey = "TEXT-" + id;
            _storage.store(textKey, text);

            string similarityKey = "SIMILARITY-" + id;
            _storage.store(similarityKey, similarity(text, id).ToString());

            await CreateTaskForRankCalculator(id);
            
            return Redirect($"summary?id={id}");
        }

        private async Task CreateTaskForRankCalculator(string id)
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
