using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;

namespace GMD.Services
{
    public class Meddra_Indications_Parse
    {
        //Parses the Meddra_All_Indications file
        public List<Meddra_Indications> ParseMeddra()
        {
            Stopwatch sw = Stopwatch.StartNew();    
            List<Meddra_Indications> symptomList = new List<Meddra_Indications>();
            string[] lines = File.ReadAllLines("sources/meddra_all_indications.tsv");

            foreach (string line in lines)
            {
                string[] elements = line.Trim().Split('\t');

                Meddra_Indications entry = new Meddra_Indications
                {
                    CID = elements[0],
                    CUI = elements[5],
                    Symptom = elements[6],
                };
                symptomList.Add(entry);

            }
            sw.Stop();
            Console.WriteLine("MeddraInd parse time : " + sw.ElapsedMilliseconds);
            return symptomList;
        }

        //Indexes all the MeddraIndications datas
        //TextField allows a parsed query to be performed within the index field. This means that Lucene won't expect a perfect fit and will rank results with a score
        //String field works as a key and Lucene will look for a perfect or almost perfect fit.
        public void indexMeddraIndicationDatas(List<Meddra_Indications> meddIndicationDatas, IndexWriter writer)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (Meddra_Indications drug in meddIndicationDatas)
            {
                Document doc = new Document();
                doc.Add(new StringField("CID_IND", drug.CID, Field.Store.YES));
                doc.Add(new StringField("CUI_IND", drug.CUI, Field.Store.YES));
                doc.Add(new TextField("name_IND", drug.Symptom, Field.Store.YES));
                writer.AddDocument(doc);
            }

            writer.Commit();
            stopwatch.Stop();
            Console.WriteLine("MeddraInd index time : " + stopwatch.ElapsedMilliseconds);   
        }
    }
}
