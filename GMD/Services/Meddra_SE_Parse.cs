using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace GMD.Services
{
    public class Meddra_SE_Parse
    {
        public List<Meddra_SE> ParseMeddra_SE()
        {
            List<Meddra_SE> symptomList = new List<Meddra_SE>();

            // Séparer les lignes en fonction des sauts de ligne
            string[] lines = File.ReadAllLines("sources/meddra_all_se.tsv");

            // Parcourir chaque ligne
            foreach (string line in lines)
            {
                // Séparer les éléments en fonction des tabulations
                string[] elements = line.Trim().Split('\t');

                Meddra_SE entry = new Meddra_SE
                {
                    Code = elements[2],
                    Symptoms = elements[5],
                    CID = elements[0],
                };

                symptomList.Add(entry);

            }
            return symptomList;
        }

        public void indexMeddraSEDatas(List<Meddra_SE> meddSEDatas, IndexWriter writer)
        {

            foreach (Meddra_SE drug in meddSEDatas)
            {
                Document doc = new Document();
                doc.Add(new StringField("CUI", drug.CID, Field.Store.YES));
                doc.Add(new StringField("HP", drug.Code, Field.Store.YES));
                doc.Add(new StringField("symptoms", drug.Symptoms, Field.Store.YES));
                writer.AddDocument(doc);
            }

            writer.Commit();
        }
    }
}
