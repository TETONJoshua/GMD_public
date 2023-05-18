using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace GMD.Services
{
    public class ChemicalParse
    {
        public List<Chemical> ParseChemical()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine("Reading chem....");
            List<Chemical> chemicals = new List<Chemical>();
            string[] lines = File.ReadAllLines("sources/chemical.sources.v5.0.tsv");
            foreach (string line in lines)
            {
                if (line.Contains("CIDm") && line.Contains("CIDs") && line.Contains("ATC"))
                {
                    string[] parts = line.Split('\t');

                    if (parts.Length == 4)
                    {
                        // Création d'un objet DataEntry et ajout à la liste
                        Chemical entry = new Chemical
                        {
                            CID = parts[1].Replace("s", "1"),
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

        public void indexChemicalsDatas(List<Chemical> ChemicalDatas, IndexWriter writer)
        {
            Stopwatch sw = Stopwatch.StartNew();
            foreach (Chemical drug in ChemicalDatas)
            {
                Document doc = new Document();
                doc.Add(new StringField("CID", drug.CID, Field.Store.YES));
                doc.Add(new StringField("ATC", drug.ATC, Field.Store.YES));
                writer.AddDocument(doc);
            }
            writer.Commit();
            sw.Stop();
            Console.WriteLine("Chem index time : " + sw.ElapsedMilliseconds);  
        }

    }
}
