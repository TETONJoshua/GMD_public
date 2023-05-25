using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;

namespace GMD.Services
{
    public class Meddra_SE_Parse
    {
       
        //Parses Meddra_All_SE file
        public List<Meddra_SE> ParseMeddra_SE()
        {

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<Meddra_SE> symptomList = new List<Meddra_SE>();

            
            string[] lines = File.ReadAllLines("sources/meddra_all_se.tsv");

            // Go through all lines
            foreach (string line in lines)
            {
                // Splits the lines on the tabs
                string[] elements = line.Trim().Split('\t');

                Meddra_SE entry = new Meddra_SE
                {
                    //Get CUI
                    Code = elements[2],
                    //Get side effect of molecule
                    Symptoms = elements[5],
                    //Get CID
                    CID = elements[0],
                };

                symptomList.Add(entry);

            }
            stopwatch.Stop();   
            Console.WriteLine("MeddraSE parse time : " +  stopwatch.ElapsedMilliseconds);
            return symptomList;
        }

        //Indexes all Meddra SE datas
        //TextField allows a parsed query to be performed within the index field. This means that Lucene won't expect a perfect fit and will rank results with a score
        //String field works as a key and Lucene will look for a perfect or almost perfect fit.
        public void indexMeddraSEDatas(List<Meddra_SE> meddSEDatas, IndexWriter writer)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();  
            foreach (Meddra_SE drug in meddSEDatas)
            {
                Document doc = new Document();
                //index CID
                doc.Add(new StringField("CID_SE", drug.CID, Field.Store.YES));
                //index CUI
                doc.Add(new StringField("CUI_SE", drug.Code, Field.Store.YES));      
                //index side effect
                doc.Add(new TextField("name_SE", drug.Symptoms, Field.Store.YES));
                writer.AddDocument(doc);
            }
            writer.Commit();
            stopwatch.Stop();
            Console.WriteLine("MeddraSE index time : " + stopwatch.ElapsedMilliseconds);
        }
    }
}
