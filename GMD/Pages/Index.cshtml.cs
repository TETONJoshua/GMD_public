using GMD.Mapping;
using GMD.Services;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;

namespace GMD.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public List<RecordDrugBankXML> recordDrugs { get; set; }
        public int countNullATC, countRecord, countNullNames, countNullTox;
        public long parseTime;
        public void OnGet()
        {
            drugBankXML dbx = new drugBankXML();
            Stopwatch sw = Stopwatch.StartNew();
            recordDrugs = dbx.parseXML(); 
            sw.Stop();
            parseTime = sw.ElapsedMilliseconds;
            countNullATC = dbx.countNullATC;
            countRecord = dbx.countRecord;
            countNullNames = dbx.countNullNames;
            countNullTox = dbx.countNullTox;
        }
    }
}