using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;

namespace GMD.Services
{
    public class MeddraParse
    {
        public List<Meddra> ParseMeddra()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Meddra> symptomList = new List<Meddra>();

            // Séparer les lignes en fonction des sauts de ligne
            string[] lines = File.ReadAllLines("sources/meddra.tsv");

            // Parcourir chaque ligne
            foreach (string line in lines)
            {
                // Séparer les éléments en fonction des tabulations
                string[] elements = line.Trim().Split('\t');

                Meddra entry = new Meddra
                {
                    Code = elements[0],
                    Symptoms = elements[3]
                };

                symptomList.Add(entry);

            }
            stopwatch.Stop();
            Console.WriteLine("Meddra parse time : " +  stopwatch.ElapsedMilliseconds);
            return symptomList;
        }

        public void indexMeddraDatas(List<Meddra> meddDatas, IndexWriter writer)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (Meddra drug in meddDatas)
            {
                Document doc = new Document();                
                doc.Add(new StringField("CUI", drug.Code, Field.Store.YES));
                //Console.WriteLine(drug.Symptoms);
                doc.Add(new TextField("name", drug.Symptoms, Field.Store.YES));
                writer.AddDocument(doc);
            }
           
            writer.Commit();
            stopwatch.Stop();
            Console.WriteLine("Meddra index time : " + stopwatch.ElapsedMilliseconds);
        }
    }
}
