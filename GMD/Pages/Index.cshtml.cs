using GMD.Mapping;
using GMD.Services;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System.Diagnostics;
using LuceneDirectory = Lucene.Net.Store.Directory;
using static Lucene.Net.Util.Packed.PackedInt32s;

namespace GMD.Pages
{
    public class IndexModel : PageModel
    {
        public const LuceneVersion luceneVersion = LuceneVersion.LUCENE_48;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        
        public void OnGet()
        {

            

            //Open the Directory using a Lucene Directory class
            string indexName = "lucene_index";
            string indexPath = Path.Combine(Environment.CurrentDirectory, indexName);

            using LuceneDirectory indexDir = FSDirectory.Open(indexPath);

            // Create an analyzer to process the text 
            Analyzer standardAnalyzer = new StandardAnalyzer(luceneVersion);

            //Create an index writer
            IndexWriterConfig indexConfig = new IndexWriterConfig(luceneVersion, standardAnalyzer);
            indexConfig.OpenMode = OpenMode.CREATE;                             // create/overwrite index
            IndexWriter writer = new IndexWriter(indexDir, indexConfig);

            brKeg dKegg = new brKeg();
            drugBankXML dbx = new drugBankXML();
            ChemicalParse chem = new ChemicalParse();
            hpo hpo = new hpo();
            MeddraParse meddra = new MeddraParse();
            Meddra_Freq_Parse meddraFreqParse = new Meddra_Freq_Parse();
            Meddra_Indications_Parse meddraIndParse = new Meddra_Indications_Parse();
            Meddra_SE_Parse meddraSEParse = new Meddra_SE_Parse();
            ominCSV ominCSV = new ominCSV();
            ominTXT ominTXT = new ominTXT();
            sqlite_Parser sqliteParser = new sqlite_Parser();


            List<RecordBrKEG> keggDatas = dKegg.parseKeg();
            List<RecordDrugBankXML> drugBankDatas = dbx.parseXML();
            List<RecordOmin> ominTxtDatas = ominTXT.ParseFile();
            List<RecordOminCSV> ominCsvDatas = ominCSV.ParseCsv();
            List<Meddra> meddraDatas = meddra.ParseMeddra();
            List<Meddra_freq> meddraFreqDatas = meddraFreqParse.ParseMeddra();
            List<Meddra_SE> meddraSeDatas = meddraSEParse.ParseMeddra_SE();
            List<Meddra_Indications> meddraIndicationsData = meddraIndParse.ParseMeddra();
            List<RecordHPO> hpoDatas = hpo.ParseHpo();

            indexXmlDatas(drugBankDatas, writer);
            indexKeggDatas(keggDatas, writer);
            
            using DirectoryReader reader = writer.GetReader(applyAllDeletes: true);
            IndexSearcher searcher = new IndexSearcher(reader);

            string symptom = "hepatitis, thrombotic thrombocytopenic purpura, idiopathic thrombocytopenic purpura, psoriasis, rheumatoid arthritis, interstitial nephritis, thyroiditis";
            Console.WriteLine("Symptomes recherchés: " + symptom);
            getSideEffectsMoleculeNames(standardAnalyzer, searcher, symptom);
            
        }

        public void indexXmlDatas(List<RecordDrugBankXML> drugBankDatas, IndexWriter writer)
        {
            
            foreach(RecordDrugBankXML drug in drugBankDatas)
            {
                Document doc = new Document();
                doc.Add(new StringField("name", drug.name, Field.Store.YES));
                doc.Add(new TextField("toxicity", drug.toxicity, Field.Store.YES));
                writer.AddDocument(doc);
            }
           
            writer.Commit();
        }

        public void indexKeggDatas(List<RecordBrKEG> keggDatas, IndexWriter writer)
        {
            
            foreach (RecordBrKEG drug in keggDatas)
            {
                Document doc = new Document();                
                doc.Add(new StringField("name", drug.medicName, Field.Store.YES));
                doc.Add(new StringField("ATC", drug.ATC, Field.Store.YES));
                writer.AddDocument(doc);
            }
           
            writer.Commit();
        }

        public void getSideEffectsMoleculeNames(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom)
        {
            QueryParser parser = new QueryParser(luceneVersion, "toxicity", standardAnalyzer);
            Query query = parser.Parse(symptom);
            TopDocs topDocs = searcher.Search(query, n: 10);         //indicate we want the first 10 results
            int i;
            for (i =0 ; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string foundName = resultDoc.Get("name");
               
                Console.WriteLine($"Result : {foundName}");
                getAtcFromName(standardAnalyzer, searcher, foundName);

            }
            Console.WriteLine(i);
        }

        public List<string> getAtcFromName(Analyzer standardAnalyzer, IndexSearcher searcher, string name)
        {
            QueryParser parser = new QueryParser(luceneVersion, "name", standardAnalyzer);
            Query query = new TermQuery(new Term("name", name));
            TopDocs topDocs = searcher.Search(query, n: 10);
            string atcCode = "";
            List<string> atcCodes = new List<string>();
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                atcCode = resultDoc.Get("ATC");
                if (atcCode != "" && atcCode!=null)
                {
                    Console.WriteLine($"ATC of result : {atcCode}" + ";");
                    atcCodes.Add(atcCode);
                }
            }
            return atcCodes;
            
        }
    }
}