using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;

namespace GMD.Services
{
    public class brKeg
    {

        public List<RecordBrKEG> parseKeg()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<RecordBrKEG> records = new List<RecordBrKEG>();
            string[] lines = File.ReadAllLines("./sources/br08303.keg");
            foreach (string line in lines)
            {
                if (line.StartsWith("E"))
                {
                    var kegInfo = line.Substring(9, line.Length-9);
                    var kegName = kegInfo.Substring(8).Split(' ');
                    string correctedName = "";
                    foreach (var subName in kegName)
                    {
                        if (!subName.StartsWith("[DG:")) { correctedName += subName + " "; }
                    }
                    correctedName = correctedName.Trim();
                    //Console.WriteLine($"Keg ATC : {kegInfo.Substring(0, 7)} ; Keg Name : {correctedName}");

                    records.Add(new RecordBrKEG(kegInfo.Substring(0, 7), correctedName));
                }
            }
            stopwatch.Stop();
            //Console.WriteLine(records.Count);
            Console.WriteLine("Kegg parse time : " + stopwatch.ElapsedMilliseconds);
            return records;
        }

        public void indexKeggDatas(List<RecordBrKEG> keggDatas, IndexWriter writer)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (RecordBrKEG drug in keggDatas)
            {
                Document doc = new Document();
                doc.Add(new StringField("drugName", drug.medicName, Field.Store.YES));
                if (drug.ATC == "B01AC04")
                {
                    Console.WriteLine(drug.medicName);
                }
                doc.Add(new StringField("ATC", drug.ATC, Field.Store.YES));

                writer.AddDocument(doc);
            }
            
            writer.Commit();
            sw.Stop();
            Console.WriteLine("Kegg index time : " + sw.ElapsedMilliseconds);
        }

    }
}
