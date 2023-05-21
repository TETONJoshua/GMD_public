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
            int MAX_RESULTS_DIS = 5;
            int MAX_RESULTS_DRUG = 5;
            using LuceneDirectory indexDir = FSDirectory.Open(indexPath);

            // Create an analyzer to process the text 
            Analyzer standardAnalyzer = new StandardAnalyzer(luceneVersion);

            //Create an index writer
            IndexWriterConfig indexConfig = new IndexWriterConfig(luceneVersion, standardAnalyzer);
            indexConfig.OpenMode = OpenMode.CREATE;                             // create/overwrite index
            IndexWriter writer = new IndexWriter(indexDir, indexConfig);

            brKeg dKegg = new brKeg(); //Done
            drugBankXML dbx = new drugBankXML(); //Done
            ChemicalParse chem = new ChemicalParse(); //Done
            hpo hpo = new hpo();
            MeddraParse meddra = new MeddraParse();
            Meddra_Freq_Parse meddraFreqParse = new Meddra_Freq_Parse();
            Meddra_Indications_Parse meddraIndParse = new Meddra_Indications_Parse();
            Meddra_SE_Parse meddraSEParse = new Meddra_SE_Parse();
            ominCSV ominCSV = new ominCSV();
            ominTXT ominTXT = new ominTXT();
            sqlite_Parser sqliteParser = new sqlite_Parser();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<RecordBrKEG> keggDatas = dKegg.parseKeg();
            List<RecordDrugBankXML> drugBankDatas = dbx.parseXML();
            List<Chemical> chemicalsDatas = chem.ParseChemical();
            List<RecordOmin> ominTxtDatas = ominTXT.ParseFile();
            List<RecordOminCSV> ominCsvDatas = ominCSV.ParseCsv();
            List<Meddra> meddraDatas = meddra.ParseMeddra();
            List<Meddra_freq> meddraFreqDatas = meddraFreqParse.ParseMeddra();
            List<Meddra_SE> meddraSeDatas = meddraSEParse.ParseMeddra_SE();
            List<Meddra_Indications> meddraIndicationsData = meddraIndParse.ParseMeddra();
            List<RecordHPO> hpoDatas = hpo.ParseHpo();
            List<sqlite> sqlitesDatas = sqliteParser.ParseSqlite();
            stopwatch.Stop();

            Console.WriteLine("Total Parse Time : " +  stopwatch.Elapsed);

            stopwatch.Restart();

            dbx.indexXmlDatas(drugBankDatas, writer);
            dKegg.indexKeggDatas(keggDatas, writer);
            chem.indexChemicalsDatas(chemicalsDatas, writer);
            hpo.indexHPODatas(hpoDatas, writer);
            ominTXT.indexOminTxtDatas(ominTxtDatas, writer);
            ominCSV.indexOminCsvDatas(ominCsvDatas, writer);
            sqliteParser.indexSqliteDatas(sqlitesDatas, writer);
            meddraFreqParse.indexMeddraFreqDatas(meddraFreqDatas, writer);
            meddraSEParse.indexMeddraSEDatas(meddraSeDatas, writer);
            meddraIndParse.indexMeddraIndicationDatas(meddraIndicationsData, writer);
            meddra.indexMeddraDatas(meddraDatas, writer);

            stopwatch.Stop();

            Console.WriteLine("Total Index Time : " + stopwatch.Elapsed);
            
            using DirectoryReader reader = writer.GetReader(applyAllDeletes: true);
            IndexSearcher searcher = new IndexSearcher(reader);

                    
            string symptom = "Loss of Nails;Erosions;Joints contractures";

            stopwatch.Restart();


            //Console.WriteLine("Search for CUI for " + symptom);
            string[] brokenSymptom = symptom.Split(";");    
            List<QueryResult> queryResults = new List<QueryResult>();
            foreach (string sympt in brokenSymptom)
            {
                Console.WriteLine("Researched symptoms : " + sympt);
                queryResults.Add(QueryManager.getQueryResult(standardAnalyzer, searcher, sympt, luceneVersion));
            }
            Dictionary<string, Disease> diseasesDict = new Dictionary<string, Disease>();
            Dictionary<string, float> diseasesDictStr = new Dictionary<string, float>();
            Dictionary<string, Drug> drugsDict = new Dictionary<string, Drug>();
            Dictionary<string, float> drugsDictStr = new Dictionary<string, float>();
            
            foreach (QueryResult queryResult in queryResults)
            {
                foreach (DiseaseResult disR in queryResult.foundDiseases) 
                { 
                    foreach(Disease disease in disR.diseases)
                    {                        
                        if (diseasesDictStr.ContainsKey(disease.diseaseName))
                        {
                             
                            diseasesDict[disease.diseaseName].score = diseasesDict[disease.diseaseName].score *(1+disR.symptomScore) ;
                            diseasesDictStr[disease.diseaseName] = diseasesDictStr[disease.diseaseName] *(1 + disR.symptomScore);
                            
                        }
                        else
                        {
                            disease.score = 1+disR.symptomScore;
                            diseasesDict.Add(disease.diseaseName, disease);
                            diseasesDictStr.Add(disease.diseaseName, disR.symptomScore);
                        }
                    }
                }
                foreach (DrugResult drugR in queryResult.foundDrugCause)
                {
                    foreach (Drug drug in drugR.drugs)
                    {
                        if (drugsDict.ContainsKey(drug.drugName))
                        {
                            drugsDict[drug.drugName].drugScore *= 10;
                            drugsDictStr[drug.drugName] *= 10;
                        }
                        else
                        {
                            drug.drugScore = 1;
                            drugsDict.Add(drug.drugName, drug);
                            drugsDictStr.Add(drug.drugName, 10);
                        }
                    }
                }
            }

            var orderedDiseases = diseasesDictStr.OrderByDescending(x => x.Value);
            var orderedDrugs = drugsDictStr.OrderByDescending(x => x.Value);
            Dictionary<string, float> diseasesResultsStr = new Dictionary<string, float>();
            List<Disease> orderedDiseasesResults = new List<Disease>();
            Dictionary<string, float> drugsResultsStr = new Dictionary<string, float>();
            List<Drug> drugsResults = new List<Drug>();
            int i = 0, j = 0;
            foreach (var disease in orderedDiseases)
            {
                if (i < MAX_RESULTS_DIS)
                {
                    foreach (string diseaseRec in diseasesDict.Keys)
                    {
                        if (disease.Key == diseaseRec)
                        {
                            diseasesDict[diseaseRec].score += disease.Value;
                            orderedDiseasesResults.Add(diseasesDict[diseaseRec]);
                        }

                    }
                }
                else
                {
                    break;
                }
                i++;
            }
            

            foreach(var disease in orderedDiseasesResults)
            {
                Console.WriteLine(" -> DISEASE NAME : " +  disease.diseaseName);
                Console.WriteLine(" -> DISEASE Score : " +  disease.score);
                Console.WriteLine(" -> Associated cure : ");
                foreach(Drug cure in disease.cures)
                {
                    Console.WriteLine("     -> Drug name  : " + cure.drugName);
                    Console.WriteLine("     -> Indication  : " + cure.indication);

                }
            }

            /*
            Console.WriteLine("************************************************************************************************************************************************");
            Console.WriteLine("       ----------        - DISEASES -         ----------");
            foreach (DiseaseResult diseaseResult in result.foundDiseases)
            {
                if (diseaseResult.diseases.Count > 0)
                {
                    Console.WriteLine("*********************************************************************************************");
                    Console.WriteLine("Matching score : " + diseaseResult.symptomScore + "\n");
                    //Console.WriteLine("Diseases found from symptom block :\n " + diseaseResult.matchingSymtom + "\n\n");

                    foreach (Disease disease in diseaseResult.diseases)
                    {
                        Console.WriteLine("Disease name : " + disease.diseaseName);
                        if (disease.diseaseFrequency != 7)
                        {
                            Console.WriteLine("     Frequency : " + disease.diseaseFrequency + "\n");
                        }
                        else
                        {
                            Console.WriteLine("     Frequency : UNKNOWN\n");
                        }
                        if (disease.cures.Count > 0)
                        {
                            Console.WriteLine("     KNOWN CURES OR TREATMENT FOR DISEASE : ");
                            foreach (Drug drug in disease.cures)
                            {
                                Console.WriteLine("         Drug name : " + drug.drugName);
                                Console.WriteLine("             Indication : \n" + drug.indication);
                            }
                        }
                    }
                    if (diseaseResult.symptomCures.Count > 0)
                    {
                        Console.WriteLine("Suggested drugs to appease the symptom : ");
                        foreach (Drug cure in diseaseResult.symptomCures)
                        {
                            Console.WriteLine("     Drug name : " + cure.drugName);
                            Console.WriteLine("         Indication : " + cure.indication);
                        }
                    }
                }
            }
            Console.WriteLine("       ----------        - DRUGS -         ----------");

            foreach (DrugResult drugResult in result.foundDrugCause)
            {
                
                foreach (Drug cure in drugResult.drugs)
                {
                    Console.WriteLine("********************************************************************************");
                    Console.WriteLine("Drug name : " + cure.drugName);
                    Console.WriteLine("     -> Frequence : " + drugResult.frequence);
                    Console.WriteLine("     -> Toxicity : " + cure.toxicity);
                }
                
            }*/
            stopwatch.Stop();
            Console.WriteLine("Query time : " + stopwatch.ElapsedMilliseconds);
        }
    }
}