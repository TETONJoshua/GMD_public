using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using System;

using System.Security.Cryptography;
using System.Xml.Linq;

namespace GMD.Services
{
    public static class QueryManager
    {
        public static void getSideEffectsMoleculeNames(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {
            Console.WriteLine("Researched symptoms : " + symptom);
            QueryParser parser = new QueryParser(luceneVersion, "toxicity", standardAnalyzer);
            Query query = parser.Parse(symptom);
            TopDocs topDocs = searcher.Search(query, n: 10);         //indicate we want the first 10 results
            int i;
            Console.WriteLine($"Molcules that can cause this as a side effects :");
            for (i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string foundName = resultDoc.Get("drugName");
                string tox= resultDoc.Get("toxicity");

                Console.WriteLine($"    -> {foundName}, \n      Score : {topDocs.ScoreDocs[i].Score}");
                Console.WriteLine($"        -> {tox}\n");
            }
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
                    Console.WriteLine($"    -> Suggested drug :");
                    Console.WriteLine($"        -> NAME : {name}");
                    Console.WriteLine($"        -> CID : {CID}");
                    Console.WriteLine($"        -> ATC : {atcCode}");
                    
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


        public static List<string> getCUIFromSymptom_INDIC(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {
            QueryParser parser = new QueryParser(luceneVersion, "symptoms", standardAnalyzer);
            Query query = parser.Parse(symptom);
            TopDocs topDocs = searcher.Search(query, n: 10);
            string CUI;
            List<string> CUIs = new List<string>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                CUI = resultDoc.Get("CUI");
                
                bool known = false;
                if (CUI != "" && CUI != null)
                {
                    Console.WriteLine(CUI);
                    foreach (string alreadyKnown in CUIs)
                    {
                        if (alreadyKnown == CUI)
                        {
                            known = true;
                            
                            break;
                        }
                    }
                    if (!known)
                    {
                        if (getCUIFromUMLS(searcher, CUI, luceneVersion, topDocs.ScoreDocs[i].Score).Count != 0)
                        {
                            if (getCIDFromCUI_INDIC(searcher, CUI, luceneVersion, topDocs.ScoreDocs[i].Score).Count != 0)
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
                            Console.WriteLine($"\n-> CUI {CUI} has been found to match your symptoms with a score of {topDocs.ScoreDocs[i].Score}, but no diseases are related to it         \n");
                        }
                        CUIs.Add(CUI);
                    }
                }
            }
            return CUIs;
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

        public static List<string> getCUIFromUMLS(IndexSearcher searcher, string UMLS, LuceneVersion luceneVersion, float score)
        {
            //string name="", classID="", HP="", def = "", synonyms="";
            Query query = new TermQuery(new Term("CUI", UMLS));
            TopDocs topDocs = searcher.Search(query, n: 10);
            List<string> names = new List<string>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string name = resultDoc.Get("name");
                bool known = false;
                foreach (string nameKnown in names)
                {
                    if (name == nameKnown)
                    {
                        known = true;
                    } 
                }
                if (name != null && !known)
                {
                    Console.WriteLine(UMLS + " : " + name);
                    names.Add(name);
                }
                /*if (name != "" && name != null)
                {
                    Console.WriteLine($"-> DISEASE : {name}                   Score : {score}");
                    Console.WriteLine($"    -> CUI : {UMLS}");
                    Console.WriteLine($"    -> definition : {def}");
                    Console.WriteLine($"    -> HP : {HP}");
                    Console.WriteLine($"    -> Class ID : {classID}");
                    Hpo.Add(name);
                }*/
            }
            return names;
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
