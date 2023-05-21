using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;

using System.Security.Cryptography;
using System.Xml.Linq;
using static Lucene.Net.Queries.Function.ValueSources.MultiFunction;

namespace GMD.Services
{
    public static class QueryManager
    {
        public static List<DrugResult> getMoleculesFromSymptoms(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {
            
            QueryParser parser = new QueryParser(luceneVersion, "toxicity", standardAnalyzer);
            //parser.DefaultOperator = QueryParser.AND_OPERATOR;
            Query query = parser.Parse(symptom);
            TopDocs topDocs = searcher.Search(query, n: 10);
            int i;
            List<DrugResult> toxicDrugs = new List<DrugResult>();
            for (i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
               
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string foundName = resultDoc.Get("drugName");
                string tox= resultDoc.Get("toxicity");
                List<Drug> drugs = new List<Drug>();
                drugs.Add(new Drug(foundName, tox, ""));
                toxicDrugs.Add(new DrugResult("Unknown", drugs));
                
            }
            toxicDrugs.AddRange(getCIDfromSE(standardAnalyzer, searcher, symptom, luceneVersion));
            return toxicDrugs;
        }
        public static List<DrugResult> getCIDfromSE(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {
            QueryParser parser = new QueryParser(luceneVersion, "name_SE", standardAnalyzer);
            parser.DefaultOperator = QueryParser.AND_OPERATOR;
            Query query = parser.Parse(symptom);
            TopDocs topDocs = searcher.Search(query, n: 10);
            int i;
            string CID_SE, CUI_SE, frequence;
            List<string> CID_SEs = new List<string>();
            List<DrugResult> drugResults = new List<DrugResult>();
            for (i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
               
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                CID_SE = resultDoc.Get("CID_SE");
                CUI_SE = resultDoc.Get("CUI_SE");
                frequence = resultDoc.Get("frequence");
                bool known = false;
                if (CID_SE != "" && CID_SE != null)
                {
                    foreach (string alreadyKnown in CID_SEs)
                    {
                        if (alreadyKnown == CID_SE)
                        {
                            known = true;
                            break;
                        }
                    }
                    if (!known)
                    {
                        CID_SEs.Add(CID_SE);
                        
                        drugResults.Add(new DrugResult(frequence, getATCFromCIDForSE(searcher, CID_SE)));
                    }
                }
            }
            return drugResults;
        }

        public static List<Drug> getATCFromCIDForSE(IndexSearcher searcher, string CID)
        {
            string atcCode;
            Query query = new TermQuery(new Term("CID", CID));
            TopDocs topDocs = searcher.Search(query, n: 10);
            List<Drug> drugs = new List<Drug>();
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {  
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                atcCode = resultDoc.Get("ATC");
                if (atcCode != "" && atcCode != null)
                {
                    drugs.AddRange(getNameFromAtcForSE(searcher, atcCode));
                }
            }
            return drugs;
        }
        public static List<Drug> getNameFromAtcForSE(IndexSearcher searcher, string atcCode)
        {
            Query query = new TermQuery(new Term("ATC", atcCode));
            TopDocs topDocs = searcher.Search(query, n: 2);
            List<Drug> drugs = new List<Drug>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string name = resultDoc.Get("drugName");
                if (name != "" && name != null)
                {
                    string tox = "";
                    Query query2 = new TermQuery(new Term("drugName", name));
                    TopDocs topDocs2 = searcher.Search(query2, n: 2);
                    for (int j = 0; i < topDocs2.ScoreDocs.Length; i++)
                    {
                        Document resultDoc2 = searcher.Doc(topDocs2.ScoreDocs[j].Doc);
                        string toxicity = resultDoc2.Get("toxicity");
                        if (toxicity != "" && toxicity !=null)
                        {
                            tox = toxicity;
                        }
                    }     
                    drugs.Add(new Drug(name, tox, ""));
                }
            }
            return drugs;
        }


        public static List<Drug> getNameFromAtc(IndexSearcher searcher, string atcCode, LuceneVersion luceneVersion, string CUI, string CID, float score)
        {
            Query query = new TermQuery(new Term("ATC", atcCode));
            TopDocs topDocs = searcher.Search(query, n: 2);
            string name;
            List<Drug> cures = new List<Drug>();
           
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                name = resultDoc.Get("drugName");
                if (name != "" && name != null)
                {
                    cures.AddRange(getIndicationFromName(searcher, name, luceneVersion));
                }
            }
            return cures;
        }

        public static List<Drug> getIndicationFromName(IndexSearcher searcher, string name, LuceneVersion luceneVersion)
        {
            string indic;
            Query query = new TermQuery(new Term("drugName", name));
            TopDocs topDocs = searcher.Search(query, n: 1);
            List<Drug> cures = new List<Drug>();
           
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                indic = resultDoc.Get("indication");
                Drug cure = new Drug(name);
                if (indic != "" && indic != null)
                {
                    cure.indication = indic;
                }
            }
            return cures;
        }

        public static List<string> getCIDFromSymptom(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {
            QueryParser parser = new QueryParser(luceneVersion, "symptoms", standardAnalyzer);
            Query query = parser.Parse(symptom);
            TopDocs topDocs = searcher.Search(query, n:100000000);
            string CID;
            List<string> CIDs = new List<string>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                CID = resultDoc.Get("CID");
                bool known = false;
                if (CID != "" && CID != null)
                {
                    foreach(string alreadyKnown in CIDs)
                    {
                        if (alreadyKnown == CID)
                        {
                            known = true;
                            break;
                        }
                    }
                    if (!known)
                    {
                        Console.WriteLine($"{CID} score : {topDocs.ScoreDocs[i].Score}");
                        CIDs.Add(CID);
                    }                  
                }
            }
            return CIDs;
        }

        public static List<Drug> getCIDFromCUI_INDIC(IndexSearcher searcher, string CUI, LuceneVersion luceneVersion, float score)
        {
            string CID;
            Query query = new TermQuery(new Term("CUI", CUI));
            TopDocs topDocs = searcher.Search(query, n: 10);
            List<string> CIDs = new List<string>();
            List<Drug> cures = new List<Drug>();
            
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                CID = resultDoc.Get("CID");
                if (CID != "" && CID != null)
                {
                    bool known = false;
                    if (CID != "" && CID != null)
                    {
                        foreach (string alreadyKnown in CIDs)
                        {
                            if (alreadyKnown == CID)
                            {
                                known = true;
                                break;
                            }
                        }
                        if (!known)
                        {
                            var curesS = getATCFromCID(searcher, CID, luceneVersion, CUI, score);
                            
                            if (curesS.Count != 0)
                            {                                
                                CIDs.Add(CID);
                                cures.AddRange(curesS);
                            }                            
                        }
                    }
                }
            }
            return cures;
        }

        public static QueryResult getQueryResult(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {
            return new QueryResult(getDiseasesFromSymptom(standardAnalyzer, searcher, symptom, luceneVersion), getMoleculesFromSymptoms(standardAnalyzer, searcher, symptom, luceneVersion) );
        }
        public static List<DiseaseResult> getDiseasesFromSymptom(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {
            QueryParser parser = new QueryParser(luceneVersion, "symptoms", standardAnalyzer);
            parser.DefaultOperator = QueryParser.AND_OPERATOR;
            Query query = parser.Parse(symptom);
            TopDocs topDocs = searcher.Search(query, n: 10);
            string UMLS;
            List<string> UMLSs = new List<string>();
            List<DiseaseResult> allDiseasesResults = new List<DiseaseResult>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                UMLS = resultDoc.Get("CUI");
                string symptoms = resultDoc.Get("symptoms");
                string definition = resultDoc.Get("definition");
                bool known = false;
                List<Disease> diseases = new List<Disease>();
                
                if (UMLS != "" && UMLS != null)
                {
                    foreach (string alreadyKnown in UMLSs)
                    {
                        if (alreadyKnown == UMLS)
                        {
                            known = true;
                            
                            break;
                        }
                    }
                    if (!known)
                    {
                        diseases.AddRange(getNameFromUMLS(searcher, UMLS, luceneVersion, topDocs.ScoreDocs[i].Score, standardAnalyzer));
                        diseases.AddRange(getTitleFromUMLS(searcher, UMLS, luceneVersion, standardAnalyzer));
                        
                        if (diseases.Count != 0)
                        {
                            foreach (Disease d in diseases)
                            {
                                d.cures = getTreatmentsForDisease(searcher, d.diseaseName, luceneVersion, standardAnalyzer);
                            }
                        }
                        UMLSs.Add(UMLS);
                        var symptomTreatments = getCIDFromCUI_INDIC(searcher, UMLS, luceneVersion, topDocs.ScoreDocs[i].Score);
                        symptomTreatments.AddRange(getTreatmentsForDisease(searcher, symptom, luceneVersion, standardAnalyzer ));
                        if (topDocs.ScoreDocs[i].Score > 2)
                        {
                            Console.WriteLine(symptoms);
                        }
                        Console.WriteLine(topDocs.ScoreDocs[i].Score);
                        allDiseasesResults.Add(new DiseaseResult(symptoms, topDocs.ScoreDocs[i].Score, diseases, symptomTreatments));
                    }
                }
            }
            allDiseasesResults.AddRange(getOmimFromSymptoms(searcher, symptom, luceneVersion, standardAnalyzer));
            return allDiseasesResults;
        }

        public static List<Disease> getNameFromUMLS(IndexSearcher searcher, string UMLS, LuceneVersion luceneVersion, float score, Analyzer standardAnalyzer)
        {
            //string name="", classID="", HP="", def = "", synonyms="";
            Query query = new TermQuery(new Term("CUI", UMLS));
            TopDocs topDocs = searcher.Search(query, n:10);
            List<string> names = new List<string>();
            List<Disease> results = new List<Disease>();
            string def = "";
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string name = resultDoc.Get("name");
               
                //Console.WriteLine(definition);
                string hp = resultDoc.Get("HP");
                //Console.WriteLine(hp);
                bool known = false;
                List<Disease> HPAnnotDiseases =new List<Disease>();
                if (hp != null)
                {
                    HPAnnotDiseases = getNamesFromHP(searcher, hp, luceneVersion, score, standardAnalyzer);
                    results.AddRange(HPAnnotDiseases);
                }                
               
                foreach (string nameKnown in names)
                {
                    if (name == nameKnown)
                    {
                        known = true;
                    }
                }
                if (name != null && !known)
                {
                    names.Add(name);
                }
            }
            //results.AddRange(names);
            string syn = "";
        
            return results;
        }

        public static List<Disease> getNamesFromHP(IndexSearcher searcher, string HP, LuceneVersion luceneVersion, float score, Analyzer standardAnalyzer)
        {
            Query query = new TermQuery(new Term("HP", HP));
            TopDocs topDocs = searcher.Search(query, n: 10);
            Dictionary<string, int> names = new  Dictionary<string, int>();
            for (int i = 0; i< topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string name = resultDoc.Get("name");
                int freq = 7;
                bool known = false;
                string strFreq = resultDoc.Get("diseaseFrequency");
                if (strFreq != null && strFreq != "None")
                {
                    //Console.WriteLine(strFreq);
                    freq = Int32.Parse(strFreq);
                }
                if (name != null)
                { 
                    foreach(var nameKnown in names)
                    {
                        //Console.WriteLine(nameKnown.Key + " ; " + name);
                        if (name.Contains(nameKnown.Key))
                        {
                            known = true;
                        }
                    }
                    if (!known)
                    {
                        names.Add(name, freq);
                    }
                   
                }
            }
            var sortednames = names.OrderBy(x => x.Value);
            List<Disease> returnedDiseases = new List<Disease>();  
            foreach (var name in sortednames)
            {
                returnedDiseases.Add(new Disease(name.Key, name.Value));
            }
            return returnedDiseases;
        }

        public static List<Disease> getTitleFromUMLS(IndexSearcher searcher, string UMLS, LuceneVersion luceneVersion, Analyzer standardAnalyzer)
        {
            Query query = new TermQuery(new Term("CUI_onto", UMLS));
            TopDocs topDocs = searcher.Search(query, n: 10);
            List<string> names = new List<string>();
            List<Disease> diseases = new List<Disease>();
            List<string> titleOmim = new List<string>();
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string classID = resultDoc.Get("classID");
                string strDisease = resultDoc.Get("name");

                if(strDisease != null && strDisease != "")
                {
                    names.Add(strDisease);
                    diseases.Add(new Disease(strDisease));
                }
            }
           
            return diseases;
        }

        public static List<DiseaseResult> getOmimFromSymptoms(IndexSearcher searcher, string symptoms, LuceneVersion luceneVersion, Analyzer standardAnalyzer)
        {
            List<DiseaseResult> diseaseResults = new List<DiseaseResult>();
            QueryParser  parser = new QueryParser(luceneVersion, "symptomsOmim", standardAnalyzer);
            parser.DefaultOperator = QueryParser.AND_OPERATOR;
            Query query = parser.Parse(symptoms);
           
            TopDocs topDocs = searcher.Search(query, n: 50);
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string sym = resultDoc.Get("symptomsOmim");
                if (topDocs.ScoreDocs[i].Score > 10)
                {
                    Console.WriteLine("OMIM : " + sym);
                }
                //Console.WriteLine(topDocs.ScoreDocs[i].Score);
                string title = resultDoc.Get("name");
                string classID = resultDoc.Get("classID");
                if (title != "" && title != null)
                {
                    List<Disease> diseases = new List<Disease>();
                    diseases.Add(new Disease(title));
                    diseaseResults.Add(new DiseaseResult(sym, topDocs.ScoreDocs[i].Score, diseases, getTreatmentsForDisease(searcher, title, luceneVersion, standardAnalyzer)));
                }
            }
            

            return diseaseResults;
        }

        public static List<Drug> getTreatmentsForDisease(IndexSearcher searcher, string diseaseName, LuceneVersion luceneVersion, Analyzer standardAnalyzer)
        {
            try
            {
                QueryParser parser = new QueryParser(luceneVersion, "indication", standardAnalyzer);
                Query query = parser.Parse(diseaseName);
                TopDocs topDocs = searcher.Search(query, n: 3);
                List<Drug> suggestedDrugs = new List<Drug>();
                for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
                {
                    Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                    string drugName = resultDoc.Get("drugName");
                    string indic = resultDoc.Get("indication");
                    if (drugName != null && topDocs.ScoreDocs[i].Score > 2)
                    {
                        suggestedDrugs.Add(new Drug(drugName, indic));
                    }
                }
                return suggestedDrugs;
            }
            catch
            {
                return new List<Drug>();
            }
        }


        public static List<Drug> getATCFromCID(IndexSearcher searcher, string CID, LuceneVersion luceneVersion, string CUI, float score)
        {
            string atcCode;
            Query query = new TermQuery(new Term("CID", CID));
            TopDocs topDocs = searcher.Search(query, n: 10);
            List<Drug> cures = new List<Drug>();
            
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                atcCode = resultDoc.Get("ATC");
                if (atcCode != "" && atcCode != null)
                {
                    cures.AddRange(getNameFromAtc(searcher, atcCode, luceneVersion, CUI, CID, score));
                }
            }
            return cures;
        }

        public static List<string> getCIDFromATC(Analyzer standardAnalyzer, IndexSearcher searcher, string atcCode, LuceneVersion luceneVersion)
        {
            Query query = new TermQuery(new Term("ATC", atcCode));
            TopDocs topDocs = searcher.Search(query, n: 10);
           
            List<string> atcCodes = new List<string>();
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                atcCode = resultDoc.Get("CID");
                if (atcCode != "" && atcCode != null)
                {
                    Console.WriteLine($"ATC of result : {atcCode}" + ";");
                    atcCodes.Add(atcCode);
                }
            }
            return atcCodes;
        }
    }
}
