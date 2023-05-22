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
            int MAX_SYMPTOMS_CURE = 5;
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

                    
            string symptom = "Large hands;Absent septum pellucidum;Limited knee extension";

            stopwatch.Restart();


            //Console.WriteLine("Search for CUI for " + symptom);
            string[] brokenSymptom = symptom.Split(";");    
            List<QueryResult> queryResults = new List<QueryResult>();
            foreach (string sympt in brokenSymptom)
            {
                Console.WriteLine("Researched symptoms  ----------------------------------------------- : " + sympt);
                queryResults.Add(QueryManager.getQueryResult(standardAnalyzer, searcher, sympt, luceneVersion));
                
            }
            //queryResults.Add(QueryManager.getQueryResult(standardAnalyzer, searcher, symptom, luceneVersion));
            Dictionary<string, Disease> diseasesDict = new Dictionary<string, Disease>();
            Dictionary<string, float> diseasesDictStr = new Dictionary<string, float>();
            Dictionary<string, int> diseasesIteration = new Dictionary<string, int>();
            Dictionary<string, Drug> drugsDict = new Dictionary<string, Drug>();
            Dictionary<string, float> drugsDictStr = new Dictionary<string, float>();
            Dictionary<string, int> drugsIteration = new Dictionary<string, int>();
            
            List<Drug> symptomsCures = new List<Drug>();

            int k = 0;
            foreach (QueryResult queryResult in queryResults)
            {
                
               
                foreach (DiseaseResult disR in queryResult.foundDiseases) 
                {
                    symptomsCures.AddRange(disR.symptomCures);
                    
                    var dis  = disR.diseases.DistinctBy(x => x.diseaseName).ToList();
                    foreach (Disease disease in dis)
                    {

                        //Console.WriteLine(disease.diseaseName);
                        
                        if (diseasesDict.ContainsKey(disease.diseaseName))
                        {
                            
                            diseasesDict[disease.diseaseName].score = diseasesDict[disease.diseaseName].score + disR.symptomScore;
                            diseasesDictStr[disease.diseaseName] = diseasesDictStr[disease.diseaseName] + disR.symptomScore;
                            diseasesIteration[disease.diseaseName]+=1;

                        }
                        else if (k == 0)
                        {
                            disease.score = disR.symptomScore;
                            diseasesDict.Add(disease.diseaseName, disease);
                            diseasesDictStr.Add(disease.diseaseName, disR.symptomScore);
                            diseasesIteration.Add(disease.diseaseName, 1);
                        }
                        
                    }
                }
                foreach (DrugResult drugR in queryResult.foundDrugCause)
                {
                    bool known = false;
                    
                    foreach (Drug drug in drugR.drugs)
                    {
                        
                        if (!known)
                        {
                            
                            if (drugsDict.ContainsKey(drug.drugName))
                            {
                                known = true;
                                //Console.WriteLine(drug.drugName);
                                drugsDict[drug.drugName].drugScore = drugsDict[drug.drugName].drugScore + drug.drugScore;
                                drugsDictStr[drug.drugName] = drugsDictStr[drug.drugName] + drug.drugScore;
                                drugsIteration[drug.drugName] += 1;
                            }
                            else if (k == 0)
                            {
                                known = true;
                                //Console.WriteLine(drug.drugName);
                                drugsIteration.Add(drug.drugName, 1);
                                drugsDict.Add(drug.drugName, drug);
                                drugsDictStr.Add(drug.drugName, drug.drugScore);
                            }
                        }                     
                       
                    }
                }
                k++;
                
            }

            var orderedDiseases = diseasesDictStr.OrderByDescending(x => x.Value);
            var orderedDrugs = drugsDictStr.OrderByDescending(x => x.Value);
            Dictionary<string, float> diseasesResultsStr = new Dictionary<string, float>();
            List<Disease> orderedDiseasesResults = new List<Disease>();
            Dictionary<string, float> drugsResultsStr = new Dictionary<string, float>();
            List<Drug> orderedDrugsResults = new List<Drug>();
            int i = 0;

            foreach (var disease in orderedDiseases)
            {
                if (diseasesIteration[disease.Key] >= k)
                {
                    if (i < MAX_RESULTS_DIS)
                    {
                        foreach (string diseaseRec in diseasesDict.Keys)
                        {
                            if (disease.Key == diseaseRec)
                            {
                                diseasesDict[diseaseRec].score = disease.Value;
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
            }
            i = 0;
            foreach (var drug in orderedDrugs)
            {
                if (drugsIteration[drug.Key] == k)
                {
                    if (i < MAX_RESULTS_DRUG)
                    {
                        foreach (string drugRec in drugsDict.Keys)
                        {
                            if (drug.Key == drugRec)
                            {
                                drugsDict[drugRec].drugScore = drug.Value;
                                orderedDrugsResults.Add(drugsDict[drugRec]);
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                    i++;
                }
                
            }
            var orderedSymptomsCures = symptomsCures.DistinctBy(x => x.drugName).OrderByDescending(x => x.drugScore);
            //foreach (var drug in orderedDrugs)

            Console.WriteLine(" ----------------------------- DISEASES\n\n");
            foreach (var disease in orderedDiseasesResults)
            {
                Console.WriteLine("\n");
                Console.WriteLine(" -> DISEASE NAME : " +  disease.diseaseName);
                Console.WriteLine(" -> DISEASE Score : " +  disease.score);
                Console.WriteLine(" -> Associated cure : ");
                foreach(Drug cure in disease.cures)
                {
                    Console.WriteLine("     -> Drug name  : " + cure.drugName);
                    Console.WriteLine("     -> Indication  : " + cure.indication);

                }
                Console.WriteLine("\n");
            }
            Console.WriteLine(" ----------------------------- DRUGS\n\n");
            foreach (var drug in orderedDrugsResults)
            {
                Console.WriteLine("\n");
                Console.WriteLine(" -> DRUG NAME : " + drug.drugName);
                Console.WriteLine(" -> DRUG Score : " + drug.drugScore);
                Console.WriteLine(" -> Toxicity : " + drug.toxicity);
                Console.WriteLine("\n");
            }

            Console.WriteLine(" ----------------------------- SYMPTOMS CURES\n\n");
            int j = 0;

            foreach (var drug in orderedSymptomsCures)
            {
                if (j < MAX_SYMPTOMS_CURE)
                {
                    Console.WriteLine("\n");
                    Console.WriteLine(" -> DRUG NAME : " + drug.drugName);
                    Console.WriteLine(" -> DRUG Score : " + drug.drugScore);
                    Console.WriteLine(" -> Indication : " + drug.toxicity);
                    Console.WriteLine("\n");
                }
                else
                {
                    break;
                }
                j++;
            }
            stopwatch.Stop();
            Console.WriteLine("Query time : " + stopwatch.ElapsedMilliseconds);
        }
    }
}