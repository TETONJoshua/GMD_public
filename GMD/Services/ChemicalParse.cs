using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace GMD.Services
{
    public class ChemicalParse
    {
        //Parses chemical source file
        public List<Chemical> ParseChemical()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine("Reading chem....");
            List<Chemical> chemicals = new List<Chemical>();
            List<string> lines = new List<string>();
            int currentLine= 0;

            using (StreamReader reader = new StreamReader("sources/chemical.sources.v5.0.tsv"))
            {
                string line;
                //datas beyond 4000 first lines are useless to us.
                while ((line = reader.ReadLine()) != null && currentLine < 4000)
                {
                    lines.Add(line);
                    currentLine++;
                }
            }

            foreach (string line in lines)
            {
                if (line.Contains("CIDm") && line.Contains("CIDs") && line.Contains("ATC"))
                {
                    string[] parts = line.Split('\t');

                    if (parts.Length == 4)
                    {
                        Chemical entry = new Chemical
                        {
                            CID = parts[0].Replace("m", "1"),
                            ATC = parts[3]
                        };
                        chemicals.Add(entry);
                    }
                }               
            }
            stopwatch.Stop();
            Console.WriteLine("Chem parse time : " + stopwatch.ElapsedMilliseconds);
            return chemicals;
        }

        //Indexes all chemical sources datas 
        //String field works as a key and Lucene will look for a perfect or almost perfect fit.
        public void indexChemicalsDatas(List<Chemical> ChemicalDatas, IndexWriter writer)
        {
            Stopwatch sw = Stopwatch.StartNew();
            foreach (Chemical drug in ChemicalDatas)
            {
                Document doc = new Document();
                doc.Add(new StringField("CID_CHEM", drug.CID, Field.Store.YES));
                
                doc.Add(new StringField("ATC_CHEM", drug.ATC, Field.Store.YES));
                writer.AddDocument(doc);
            }
            writer.Commit();
            sw.Stop();
            Console.WriteLine("Chem index time : " + sw.ElapsedMilliseconds);  
        }

    }
}
