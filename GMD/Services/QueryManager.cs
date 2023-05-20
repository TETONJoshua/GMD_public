﻿using Lucene.Net.Analysis;
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
        public static void getSideEffectsMoleculeNames(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {
            Console.WriteLine("Researched symptoms : " + symptom);
            QueryParser parser = new QueryParser(luceneVersion, "toxicity", standardAnalyzer);
            parser.DefaultOperator = QueryParser.AND_OPERATOR;
            Query query = parser.Parse(symptom);
            TopDocs topDocs = searcher.Search(query, n: 10);
            int i;
            Console.WriteLine($"Molecules that can cause this as a side effects :");
            for (i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string foundName = resultDoc.Get("drugName");
                string tox= resultDoc.Get("toxicity");

                Console.WriteLine($"    -> {foundName}, \n      Score : {topDocs.ScoreDocs[i].Score}");
                Console.WriteLine($"        -> {tox}\n");
            }
            getCIDfromSE(standardAnalyzer, searcher, symptom, luceneVersion);
        }
        public static void getCIDfromSE(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {
            Console.WriteLine("Researched symptoms : " + symptom);
            QueryParser parser = new QueryParser(luceneVersion, "name_SE", standardAnalyzer);
            parser.DefaultOperator = QueryParser.AND_OPERATOR;
            Query query = parser.Parse(symptom);
            TopDocs topDocs = searcher.Search(query, n: 5000);
            int i;
            Console.WriteLine($"Molecules that can cause this as a side effects (from meddra) :");
            string CID_SE, CUI_SE, frequence;
            List<string> CID_SEs = new List<string>();
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
                        
                        getATCFromCIDForSE(searcher, CID_SE, luceneVersion, CUI_SE, 0, frequence);
                    }
                }
            }
        }

        public static List<string> getATCFromCIDForSE(IndexSearcher searcher, string CID, LuceneVersion luceneVersion, string CUI, float score, string frequence)
        {
            string atcCode;
            Query query = new TermQuery(new Term("CID", CID));
            TopDocs topDocs = searcher.Search(query, n: 10);
            List<string> indications = new List<string>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {  
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                atcCode = resultDoc.Get("ATC");
                if (atcCode != "" && atcCode != null)
                {
                    if (getNameFromAtcForSE(searcher, atcCode, luceneVersion, CUI, CID, score, frequence).Count != 0)
                    {
                        indications.Add(atcCode);
                    }

                }
            }
            return indications;
        }
        public static List<string> getNameFromAtcForSE(IndexSearcher searcher, string atcCode, LuceneVersion luceneVersion, string CUI, string CID, float score, string frequence)
        {
            Query query = new TermQuery(new Term("ATC", atcCode));
            TopDocs topDocs = searcher.Search(query, n: 2);
            string name;
            List<string> names = new List<string>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                name = resultDoc.Get("drugName");
                if (name != "" && name != null)
                {
                    Console.WriteLine($"         -> This drug have this symptom as side effect :             Score : {topDocs.ScoreDocs[i].Score}");
                    Console.WriteLine($"            -> NAME : {name}");
                    Console.WriteLine($"            -> CID : {CID}");
                    Console.WriteLine($"            -> ATC : {atcCode}");
                    if (frequence == null)
                    {
                        frequence = "unknow";
                    }
                    Console.WriteLine($"            -> FREQ : {frequence}");
                    names.Add(name);

                }
            }
            return names;
        }


        public static List<string> getNameFromAtc(IndexSearcher searcher, string atcCode, LuceneVersion luceneVersion, string CUI, string CID, float score)
        {
            Query query = new TermQuery(new Term("ATC", atcCode));
            TopDocs topDocs = searcher.Search(query, n: 2);
            string name;
            List<string> names = new List<string>();
           
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                name = resultDoc.Get("drugName");
                if (name != "" && name != null)
                {
                    Console.WriteLine($"         -> Suggested drug to treat this symptom :             Score : {topDocs.ScoreDocs[i].Score}");
                    Console.WriteLine($"            -> NAME : {name}");
                    Console.WriteLine($"            -> CID : {CID}");
                    Console.WriteLine($"            -> ATC : {atcCode}");
                    
                    getIndicationFromName(searcher, name, luceneVersion);
                    names.Add(name);
                    
                }
            }
            return names;
        }

        public static List<string> getIndicationFromName(IndexSearcher searcher, string name, LuceneVersion luceneVersion)
        {
            string indic;
            Query query = new TermQuery(new Term("drugName", name));
            TopDocs topDocs = searcher.Search(query, n: 1);
            List<string> indications = new List<string>();
           
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Console.WriteLine($"        -> Indication : ");
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                indic = resultDoc.Get("indication");
                if (indic != "" && indic != null)
                {
                    Console.WriteLine($"          -> {indic}\n");
                    indications.Add(indic);
                }
            }
            return indications;
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

        public static List<string> getCIDFromCUI_INDIC(IndexSearcher searcher, string CUI, LuceneVersion luceneVersion, float score)
        {
            string CID;
            Query query = new TermQuery(new Term("CUI", CUI));
            TopDocs topDocs = searcher.Search(query, n: 10);
            List<string> CIDs = new List<string>();
            
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
                            if (getATCFromCID(searcher, CID, luceneVersion, CUI, score).Count != 0)
                            {                                
                                CIDs.Add(CID);
                            }                            
                        }
                    }
                }
            }
            return CIDs;
        }


        public static List<string> getUMLSFromSymptom_INDIC(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {
            QueryParser parser = new QueryParser(luceneVersion, "symptoms", standardAnalyzer);
            //parser.DefaultOperator = QueryParser.AND_OPERATOR;
            Query query = parser.Parse(symptom);
            TopDocs topDocs = searcher.Search(query, n: 5);
            string UMLS;
            List<string> UMLSs = new List<string>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                UMLS = resultDoc.Get("CUI");
                string symptoms = resultDoc.Get("symptoms");
                string definition = resultDoc.Get("definition");
                //Console.WriteLine(symptoms);
                bool known = false;
                if (UMLS != "" && UMLS != null)
                {
                    //Console.WriteLine(CUI);
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
                        Console.WriteLine("Found from Symptoms : " + symptoms + $"                   Score : {topDocs.ScoreDocs[i].Score} ; " + " UMLS/CUI : " + UMLS) ;
                        Console.WriteLine("     -> Definition : " + definition ) ;
                        Console.WriteLine("     -> Assiociated to deseases : ");
                        if (getNameFromUMLS(searcher, UMLS, luceneVersion, topDocs.ScoreDocs[i].Score, standardAnalyzer).Count != 0)
                        {
                            if (getCIDFromCUI_INDIC(searcher, UMLS, luceneVersion, topDocs.ScoreDocs[i].Score).Count != 0)
                            {
                                //Console.WriteLine($"{CUI} score : {topDocs.ScoreDocs[i].Score}");
                                
                                
                            }
                            else
                            {
                                Console.WriteLine("     -> No suggested drugs for this disease");
                               
                            }

                            Console.WriteLine("\n           --------------------------------------------------          \n");
                            
                        }
                        else
                        {
                            Console.WriteLine($"\n-> CUI {UMLS} has been found to match your symptoms with a score of {topDocs.ScoreDocs[i].Score}, but no diseases are related to it         \n");
                        }
                        UMLSs.Add(UMLS);
                    }
                }
            }
            return UMLSs;
        }

        public static List<string> getNameFromUMLS(IndexSearcher searcher, string UMLS, LuceneVersion luceneVersion, float score, Analyzer standardAnalyzer)
        {
            //string name="", classID="", HP="", def = "", synonyms="";
            Query query = new TermQuery(new Term("CUI", UMLS));
            TopDocs topDocs = searcher.Search(query, n:10);
            List<string> names = new List<string>();
            List<string> results = new List<string>();
            string def = "";
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string name = resultDoc.Get("name");
                
                //Console.WriteLine(definition);
                string hp = resultDoc.Get("HP");
                //Console.WriteLine(hp);
                bool known = false;
                List<string> HPAnnotNames =new List<string>();
                if (hp != null)
                {
                    HPAnnotNames = getNamesFromHP(searcher, hp, luceneVersion, score, standardAnalyzer);
                    results.AddRange(HPAnnotNames);
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
            results.AddRange(names);
            string syn = "";
            return results;
        }

        public static List<string> getNamesFromHP(IndexSearcher searcher, string HP, LuceneVersion luceneVersion, float score, Analyzer standardAnalyzer)
        {
            Query query = new TermQuery(new Term("HP", HP));
            TopDocs topDocs = searcher.Search(query, n: 20);
            Dictionary<string, int> names = new  Dictionary<string, int>();
            for (int i = 0; i< topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string name = resultDoc.Get("name");
                int freq = 7;
                string strFreq = resultDoc.Get("diseaseFrequency");
                if (strFreq != null && strFreq != "None")
                {
                    //Console.WriteLine(strFreq);
                    freq = Int32.Parse(strFreq);
                }
                if (name != null)
                {                    
                    names.Add(name, freq);
                }
            }
            var sortednames = names.OrderBy(x => x.Value);
            List<string> returnedNames = new List<string>();    
            foreach (var name in sortednames)
            {
                returnedNames.Add(name.Key);
                Console.WriteLine($"            -> DISEASE : {name.Key}         Frequency : {name.Value}");
                getTreatmentsForDisease(searcher,  name.Key, luceneVersion, standardAnalyzer);
            }

            return returnedNames;
        }

        public static List<string> getTreatmentsForDisease(IndexSearcher searcher, string diseaseName, LuceneVersion luceneVersion, Analyzer standardAnalyzer)
        {
            QueryParser parser = new QueryParser(luceneVersion, "indication", standardAnalyzer);
            Query query = parser.Parse(diseaseName);
            TopDocs topDocs = searcher.Search(query, n: 3);
            List<string> suggestedDrugs = new List<string>();
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string drugName = resultDoc.Get("drugName");
                string indic = resultDoc.Get("indication");
                if (drugName != null && topDocs.ScoreDocs[i].Score>2)
                {
                    Console.WriteLine($"                    -> Drug : {drugName}, with score : {topDocs.ScoreDocs[i].Score}");
                    Console.WriteLine($"                        -> indication : {indic}");
                    suggestedDrugs.Add(drugName);
                }
            }
            return suggestedDrugs;
        }


        public static List<string> getATCFromCID(IndexSearcher searcher, string CID, LuceneVersion luceneVersion, string CUI, float score)
        {
            string atcCode;
            Query query = new TermQuery(new Term("CID", CID));
            TopDocs topDocs = searcher.Search(query, n: 10);
            List<string> indications = new List<string>();
            
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                atcCode = resultDoc.Get("ATC");
                if (atcCode != "" && atcCode != null)
                {
                    if (getNameFromAtc(searcher, atcCode, luceneVersion, CUI, CID, score).Count != 0)
                    {
                        indications.Add(atcCode);
                    }
                    
                }
            }
            return indications;
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

 

        public static void getGenOmimbyCUI(IndexSearcher searcher, string classID, LuceneVersion luceneVersion)
        {
            string cID, title;
            Query query = new TermQuery(new Term("Number", classID));
            TopDocs topDocs = searcher.Search(query, n: 20);
            List<string> disease = new List<string>();
            Console.WriteLine($"Linked disease : ");
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                title = resultDoc.Get("title");
                if (title != "" && title != null)
                {
                    Console.WriteLine($"    -> {title}");
                }
            }
        }

        public static void getGenOmimbyHP(IndexSearcher searcher, string HP, LuceneVersion luceneVersion)
        {
            string cID, title;
            Query query = new TermQuery(new Term("HP", HP));
            TopDocs topDocs = searcher.Search(query, n: 20);
            List<string> disease = new List<string>();
            Console.WriteLine($"Linked disease : ");
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                title = resultDoc.Get("title");
                if (title != "" && title != null)
                {
                    Console.WriteLine($"    -> {title}");
                }
            }
        }
    }
}