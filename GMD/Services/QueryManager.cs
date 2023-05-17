using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
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
                string foundName = resultDoc.Get("name");

                Console.WriteLine($"    -> {foundName}, \n      Score : {topDocs.ScoreDocs[i].Score}");
            }
           //Console.WriteLine(i);
        }

        public static List<string> getNameFromAtc(IndexSearcher searcher, string atcCode, LuceneVersion luceneVersion)
        {
            Query query = new TermQuery(new Term("ATC", atcCode));
            TopDocs topDocs = searcher.Search(query, n: 10);
            string name;
            List<string> names = new List<string>();
           
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                name = resultDoc.Get("name");
                if (name != "" && name != null)
                {
                    Console.WriteLine($"{name}");
                    names.Add(name);
                }
            }
            return names;
        }

        public static List<string> getIndicationFromName(IndexSearcher searcher, string name, LuceneVersion luceneVersion)
        {
            string indic;
            Query query = new TermQuery(new Term("name", name));
            TopDocs topDocs = searcher.Search(query, n: 1);
            List<string> indications = new List<string>();
            Console.WriteLine($"Indicated when : ");
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                indic = resultDoc.Get("indication");
                if (indic != "" && indic != null)
                {
                    Console.WriteLine($"    -> {indic}");
                    indications.Add(indic);
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

        public static List<string> getHpoUMLSFromCui(IndexSearcher searcher, string UMLS, LuceneVersion luceneVersion)
        {
            string name, classID, HP;
            Query query = new TermQuery(new Term("CUI", UMLS));
            TopDocs topDocs = searcher.Search(query, n: 20);
            List<string> Hpo = new List<string>();
            Console.WriteLine($"Linked disease : ");
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                //read back a doc from results
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                name = resultDoc.Get("name");
                classID = resultDoc.Get("classID");
                HP = resultDoc.Get("HP");
                if (name != "" && name != null)
                {
                    Console.WriteLine($"    -> {name}");
                }
                if (HP != "" && HP != null)
                {
                    Console.WriteLine($"    -> {HP}");
                    Hpo.Add(HP);
                }
                if (classID != "" && classID != null)
                {
                    Console.WriteLine($"    -> {classID}");
                    Hpo.Add(classID);
                }
            }
            return Hpo;
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
