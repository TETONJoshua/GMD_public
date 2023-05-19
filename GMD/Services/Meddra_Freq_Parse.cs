using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;

namespace GMD.Services
{
    public class Meddra_Freq_Parse
    {
        public List<Meddra_freq> ParseMeddra()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<Meddra_freq> symptomList = new List<Meddra_freq>();

            // Séparer les lignes en fonction des sauts de ligne
            string[] lines = File.ReadAllLines("sources/meddra_freq.tsv");

            // Parcourir chaque ligne
            foreach (string line in lines)
            {
                // Séparer les éléments en fonction des tabulations
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
