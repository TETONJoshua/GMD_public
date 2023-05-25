using GMD.Services;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace GMD.Pages
{
    public class affichageModel : PageModel
    {
        public const LuceneVersion luceneVersion = LuceneVersion.LUCENE_48;
        private readonly ILogger<IndexModel> _logger;
        internal List<Disease>? diseases;
        internal List<Drug>? drugs;
        internal List<Drug>? drugsCure;
        internal string symptoms;
        internal string queryTime;

        public void OnGet()
        {

        }

        public IActionResult OnPost()
        {

            string indexName = "lucene_index";
            string indexPath = Path.Combine(Environment.CurrentDirectory, indexName);
            int MAX_RESULTS_DIS = 1000;
            int MAX_RESULTS_DRUG = 1000;
            int MAX_SYMPTOMS_CURE = 1000;
            using LuceneDirectory indexDir = FSDirectory.Open(indexPath);

            // Create an analyzer to process the text 
            Analyzer standardAnalyzer = new StandardAnalyzer(luceneVersion);

            //Create an index writer
            IndexWriterConfig indexConfig = new IndexWriterConfig(luceneVersion, standardAnalyzer);
            indexConfig.OpenMode = OpenMode.APPEND;
            IndexWriter writer = new IndexWriter(indexDir, indexConfig);
            using DirectoryReader reader = writer.GetReader(applyAllDeletes: true);
            IndexSearcher searcher = new IndexSearcher(reader);
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("Total Parse Time : " + stopwatch.Elapsed);

            stopwatch.Restart();

            string symptom = Request.Form["symptom"];

            stopwatch.Restart();

            string[] brokenSymptom = symptom.Split(";");
            List<QueryResult> queryResults = new List<QueryResult>();
            foreach (string sympt in brokenSymptom)
            {
                if (sympt != "")
                {
                    Console.WriteLine("Researched symptoms  ----------------------------------------------- : " + sympt);
                    queryResults.Add(QueryManager.getQueryResult(standardAnalyzer, searcher, sympt, luceneVersion));
                }         
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
                    var dis = disR.diseases.DistinctBy(x => x.diseaseName).ToList();
                    symptomsCures.AddRange(disR.symptomCures);
                    foreach (Disease disease in dis)
                    {
                      
                        if (diseasesDict.ContainsKey(disease.diseaseName))
                        {

                            diseasesDict[disease.diseaseName].score = diseasesDict[disease.diseaseName].score + disR.symptomScore;
                            diseasesDictStr[disease.diseaseName] = diseasesDictStr[disease.diseaseName] + disR.symptomScore;
                            diseasesIteration[disease.diseaseName] += 1;

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
            var orderedSymptomsCures = symptomsCures.DistinctBy(x => x.drugName).OrderByDescending(x => x.drugScore).ToList();
            
            stopwatch.Stop();
            Console.WriteLine("Query time : " + stopwatch.ElapsedMilliseconds);
            queryTime = stopwatch.ElapsedMilliseconds.ToString();
            diseases = orderedDiseasesResults;
            foreach (var disease in orderedDiseasesResults) 
            {
                if (disease.diseaseName.StartsWith("#") || disease.diseaseName.StartsWith("%")){
                    disease.diseaseName = disease.diseaseName.Remove(0, 7);
                }
                if (disease.diseaseName.StartsWith("0") || 
                    disease.diseaseName.StartsWith("1") ||
                    disease.diseaseName.StartsWith("2") ||
                    disease.diseaseName.StartsWith("3") ||
                    disease.diseaseName.StartsWith("4") ||
                    disease.diseaseName.StartsWith("5") ||
                    disease.diseaseName.StartsWith("6") ||
                    disease.diseaseName.StartsWith("7") ||
                    disease.diseaseName.StartsWith("8") ||
                    disease.diseaseName.StartsWith("9"))
                {
                    disease.diseaseName = disease.diseaseName.Remove(0, 6);
                }
                if (disease.diseaseName.Contains(";"))
                {
                    disease.diseaseName = disease.diseaseName.Split(';')[0];
                }

            }
            drugs = orderedDrugsResults;
            symptoms = symptom;
            drugsCure = orderedSymptomsCures;
            writer.Dispose();
            return Page();
        }
    }
}
