using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Valuator.Pages
{
    public class SummaryModel : PageModel
    {
        private readonly ILogger<SummaryModel> _logger;
        private Storage _storage;
        public SummaryModel(ILogger<SummaryModel> logger, Storage storage)
        {
            _logger = logger;
            _storage = storage;
        }

        public double Rank { get; set; }
        public double Similarity { get; set; }

        public void OnGet(string id)
        {
            _logger.LogDebug(id);

            var counter = 0;
            var rank = _storage.value("RANK-" + id);
            while (rank.Length == 0 && counter < 100)
            {
                Thread.Sleep(100);
                rank = _storage.value("RANK-" + id);
                ++counter;
            }
            if (rank.Length == 0)
            {
                _logger.LogWarning($"rank for id {id} does not found");
            }

            Rank = Math.Round(Convert.ToDouble(rank), 3);
            Similarity = Convert.ToDouble(_storage.value("SIMILARITY-" + id.ToString()));

        }
    }
}
