using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;

namespace GMD.Services
{
    public class Meddra_Freq_Parse
    {
        //Parses the Meddra_all_freq File
        public List<Meddra_freq> ParseMeddra()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<Meddra_freq> symptomList = new List<Meddra_freq>();

            string[] lines = File.ReadAllLines("sources/meddra_freq.tsv");

            // Go through each lines
            foreach (string line in lines)
            {
                // Splits line on tabs
                string[] elements = line.Trim().Split('\t');

                Meddra_freq entry = new Meddra_freq
                {
                    Code = elements[2],
                    Symptoms = elements[9],
                    CID = elements[0],
                    freq = elements[5],
                };

                symptomList.Add(entry);

            }
            stopwatch.Stop();
            Console.WriteLine("MeddraFreq parse time : " + stopwatch.ElapsedMilliseconds);
            return symptomList;
        }

        //Indexes all MeddraFreq datas
        //TextField allows a parsed query to be performed within the index field. This means that Lucene won't expect a perfect fit and will rank results with a score
        //String field works as a key and Lucene will look for a perfect or almost perfect fit.
        public void indexMeddraFreqDatas(List<Meddra_freq> meddFreqDatas, IndexWriter writer)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (Meddra_freq drug in meddFreqDatas)
            {
                Document doc = new Document();
                doc.Add(new StringField("CID_SE", drug.CID, Field.Store.YES));
                doc.Add(new StringField("CUI_SE", drug.Code, Field.Store.YES));
                doc.Add(new TextField("frequence", drug.freq, Field.Store.YES));
                doc.Add(new TextField("name_SE", drug.Symptoms, Field.Store.YES));
                writer.AddDocument(doc);
            }

            writer.Commit();
            stopwatch.Stop();
            Console.WriteLine("MeddraFreq : " + stopwatch.ElapsedMilliseconds);

        }
    }
}
