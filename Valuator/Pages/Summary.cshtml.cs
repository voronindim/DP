using System;
using System.Collections.Generic;
using System.Linq;
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

            //TODO: проинициализировать свойства Rank и Similarity сохранёнными в БД значениями
            Rank = Convert.ToDouble(_storage.value("RANK-" + id.ToString()));
            Similarity = Convert.ToDouble(_storage.value("SIMILARITY-" + id.ToString()));

        }
    }
}
