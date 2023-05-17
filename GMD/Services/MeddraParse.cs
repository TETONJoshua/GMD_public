using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace GMD.Services
{
    public class MeddraParse
    {
        public List<Meddra> ParseMeddra()
        {
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
            return symptomList;
        }

        public void indexMeddraDatas(List<Meddra> meddDatas, IndexWriter writer)
        {

            foreach (Meddra drug in meddDatas)
            {
                Document doc = new Document();
                doc.Add(new StringField("HP", drug.Code, Field.Store.YES));
                doc.Add(new StringField("symptoms", drug.Symptoms, Field.Store.YES));
                writer.AddDocument(doc);
            }

            writer.Commit();
        }
    }
}
