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
                string tox= resultDoc.Get("toxicity");

                Console.WriteLine($"    -> {foundName}, \n      Score : {topDocs.ScoreDocs[i].Score}");
                Console.WriteLine($"        -> {tox}\n");
            }
        }

        public static List<string> getNameFromAtc(IndexSearcher searcher, string atcCode, LuceneVersion luceneVersion)
        {
            Query query = new TermQuery(new Term("ATC", atcCode));
            TopDocs topDocs = searcher.Search(query, n: 100);
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
    }
}
