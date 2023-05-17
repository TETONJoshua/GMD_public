using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;

namespace GMD.Services
{
    public class ominTXT
    {

        public List<RecordOmin> ParseFile()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<RecordOmin> records = new List<RecordOmin>();
            RecordOmin currentRecord = null;

            string[] lines = File.ReadAllLines("sources/omim.txt");
            int lineIndex = 0;
            foreach (string line in lines)
            {
                
                if (line.StartsWith("*RECORD*"))
                {
                    currentRecord = new RecordOmin();
                    records.Add(currentRecord);
                }
                else if (line.StartsWith("*FIELD* NO"))
                {
                    currentRecord.Number = GetFieldValue(lines, lineIndex);
                }
                else if (line.StartsWith("*FIELD* TI"))
                {
                    currentRecord.Title = GetFieldValue(lines, lineIndex);
                }
                else if (line.StartsWith("*FIELD* CS"))
                {
                    currentRecord.ClinicalFeatures = GetFieldValue(lines, lineIndex);
                }
            }
            stopwatch.Stop();
            Console.WriteLine("OMIM_TXT parse time : " + stopwatch.ElapsedMilliseconds);
            return records;
        }

        public static string GetFieldValue(string[] lines, int currentLineIndex)
        {
            string fieldValue = string.Empty;

            for (int i = currentLineIndex + 1; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.StartsWith("*FIELD*"))
                {
                    break;
                }

                fieldValue += line.Trim() + Environment.NewLine;
            }

            return fieldValue.Trim();
        }

        public void indexOminTxtDatas(List<RecordOmin> drugBankDatas, IndexWriter writer)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (RecordOmin drug in drugBankDatas)
            {
                Document doc = new Document();
                doc.Add(new StringField("title", drug.Title, Field.Store.YES));
                doc.Add(new StringField("classID", drug.Number, Field.Store.YES));
                doc.Add(new StringField("title", drug.ClinicalFeatures, Field.Store.YES));
                writer.AddDocument(doc);
            }

            writer.Commit();
            stopwatch.Stop();
            Console.WriteLine("OMIM_TXTindex time : " + stopwatch.ElapsedMilliseconds);
        }
    }
}
