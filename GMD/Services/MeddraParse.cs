using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;

namespace GMD.Services
{
    public class MeddraParse
    {
        //Parses MeddraAll file
        public List<Meddra> ParseMeddra()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Meddra> symptomList = new List<Meddra>();

            string[] lines = File.ReadAllLines("sources/meddra.tsv");

            // Go through each lines
            foreach (string line in lines)
            {
                // Split lines on tabs
                string[] elements = line.Trim().Split('\t');

                Meddra entry = new Meddra
                {
                    //Get CUI
                    Code = elements[0],
                    //Get symptom to cure
                    Symptoms = elements[3]
                };

                symptomList.Add(entry);

            }
            stopwatch.Stop();
            Console.WriteLine("Meddra parse time : " +  stopwatch.ElapsedMilliseconds);
            return symptomList;
        }

        //Indexes meddra datas in Lucene rep
        //TextField allows a parsed query to be performed within the index field. This means that Lucene won't expect a perfect fit and will rank results with a score
        //String field works as a key and Lucene will look for a perfect or almost perfect fit.
        public void indexMeddraDatas(List<Meddra> meddDatas, IndexWriter writer)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (Meddra drug in meddDatas)
            {
                Document doc = new Document();
                //Index CUI
                doc.Add(new StringField("CUI_IND_MED", drug.Code, Field.Store.YES));
                //Index symptom
                doc.Add(new TextField("name_IND", drug.Symptoms, Field.Store.YES));
                writer.AddDocument(doc);
            }
           
            writer.Commit();
            stopwatch.Stop();
            Console.WriteLine("Meddra index time : " + stopwatch.ElapsedMilliseconds);
        }
    }
}
