using GMD.Mapping;
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
                if (line.StartsWith("F"))
                {
                    var kegInfo = line.Substring(11, line.Length-11).Split("  ");
                    //Console.WriteLine($"Keg ATC : {kegInfo[0]} ; Keg Name : {kegInfo[1]}");
                    records.Add(new RecordBrKEG(kegInfo[0], kegInfo[1]));
                }
            }
            stopwatch.Stop();
            //Console.WriteLine(records.Count);
            //Console.WriteLine(stopwatch.ElapsedMilliseconds);
            return records;
        }
    }
}
