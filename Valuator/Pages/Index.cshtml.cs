using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

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
        public IActionResult OnPost(string text)
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            string textKey = "TEXT-" + id;
            _storage.store(textKey, text);

            string rankKey = "RANK-" + id;
            _storage.store(rankKey, rank(text).ToString());

            string similarityKey = "SIMILARITY-" + id;
            _storage.store(similarityKey, similarity(text, id).ToString());

            return Redirect($"summary?id={id}");
        }

        private double rank(string text) { 
            var counter = 0;
            foreach ( var ch in text) { 
                if (!Char.IsLetter(ch)) { 
                    counter++;
                }
            }
            return Convert.ToDouble(counter / Convert.ToDouble(text.Length));
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
