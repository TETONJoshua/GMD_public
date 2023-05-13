﻿using GMD.Mapping;
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
                    correctedName.Remove(correctedName.Length - 1);
                    Console.WriteLine($"Keg ATC : {kegInfo.Substring(0, 7)} ; Keg Name : {correctedName}");

                    records.Add(new RecordBrKEG(kegInfo.Substring(0, 7), correctedName));
                }
            }
            stopwatch.Stop();
            Console.WriteLine(records.Count);
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            return records;
        }
    }
}