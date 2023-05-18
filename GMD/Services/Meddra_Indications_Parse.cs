using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;

namespace GMD.Services
{
    public class Meddra_Indications_Parse
    {
        public List<Meddra_Indications> ParseMeddra()
        {
            Stopwatch sw = Stopwatch.StartNew();    
            List<Meddra_Indications> symptomList = new List<Meddra_Indications>();

            // Séparer les lignes en fonction des sauts de ligne
            string[] lines = File.ReadAllLines("sources/meddra_all_indications.tsv");

            // Parcourir chaque ligne
            foreach (string line in lines)
            {
                // Séparer les éléments en fonction des tabulations
                string[] elements = line.Trim().Split('\t');

                Meddra_Indications entry = new Meddra_Indications
                {
                    Code = elements[1],
                    Symptoms = elements[3],
                    CID = elements[0],
                };

                symptomList.Add(entry);

            }
            sw.Stop();
            Console.WriteLine("MeddraInd parse time : " + sw.ElapsedMilliseconds);
            return symptomList;
        }

        public void indexMeddraIndicationDatas(List<Meddra_Indications> meddIndicationDatas, IndexWriter writer)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (Meddra_Indications drug in meddIndicationDatas)
            {
                Document doc = new Document();
                doc.Add(new TextField("CID", drug.CID, Field.Store.YES));
                doc.Add(new TextField("CUI", drug.Code, Field.Store.YES));
                doc.Add(new TextField("symptoms", drug.Symptoms, Field.Store.YES));
                writer.AddDocument(doc);
            }

            writer.Commit();
            stopwatch.Stop();
            Console.WriteLine("MeddraInd index time : " + stopwatch.ElapsedMilliseconds);   
        }
    }
}
