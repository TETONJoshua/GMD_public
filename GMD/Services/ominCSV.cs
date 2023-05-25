using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;

namespace GMD.Services
{
    public class ominCSV
    {
        //Parses the omim_onto.csv file
        public List<RecordOminCSV> ParseCsv()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<RecordOminCSV> RecordOminCSVs = new List<RecordOminCSV>();

            using (StreamReader reader = new StreamReader("sources/omim_onto.csv"))
            {
                string line;
                bool isFirstLine = true;

                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue; // Ignores the first line (headers for the column fields)
                    }

                    string[] values = line.Split('!');

                    RecordOminCSV RecordOminCSV = new RecordOminCSV
                    {
                        ClassId = values[0].Replace("http://purl.bioontology.org/ontology/OMIM/",""),
                        Cui = values[5],
                        Synonyms = values[2],
                        PreferredLabel = values[1]
                    };

                    RecordOminCSVs.Add(RecordOminCSV);
                }
            }
            stopwatch.Stop();
            Console.WriteLine("OMIM_CSV : " + stopwatch.ElapsedMilliseconds);
            return RecordOminCSVs;
        }


        //Indexes OMIM in Lucene rep (do not care for the typo in the method name)
        //TextField allows a parsed query to be performed within the index field. This means that Lucene won't expect a perfect fit and will rank results with a score
        //String field works as a key and Lucene will look for a perfect or almost perfect fit.
        public void indexOminCsvDatas(List<RecordOminCSV> ominCSVdatas, IndexWriter writer)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (RecordOminCSV drug in ominCSVdatas)
            {
                Document doc = new Document();
                doc.Add(new StringField("classID_onto", drug.ClassId, Field.Store.YES));
                doc.Add(new StringField("CUI_onto", drug.Cui, Field.Store.YES));
                doc.Add(new StringField("synonyms", drug.Synonyms, Field.Store.YES));
                doc.Add(new TextField("name_onto", drug.PreferredLabel, Field.Store.YES));
                writer.AddDocument(doc);
            }

            writer.Commit();
            stopwatch.Stop();
            Console.WriteLine("OMIM_CSV index time : " + stopwatch.ElapsedMilliseconds);
        }
    }
}
