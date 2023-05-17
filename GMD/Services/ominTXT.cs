using GMD.Mapping;
using J2N.Numerics;
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
            List<int> index= new List<int>();
            List<int> indexTI= new List<int>();
            List<int> indexCS= new List<int>();
            string[] lines = File.ReadAllLines("sources/omim.txt");
            int lineIndex = 0;
            foreach (string line in lines)
            {
              if (line.StartsWith("*RECORD*"))
                {
                    index.Add(lineIndex);
                }
              lineIndex++;
            }
            string number="", title="";
            for (int i=0; i<index.Count-1;  i++)
            {
                records.Add(GetFieldValue(lines, index[i], index[i + 1]));               
            }
            //Console.WriteLine("Number : " + records[0].Number + " Title : " + records[0].Title + " CS : " + records[0].ClinicalFeatures);
            stopwatch.Stop();
            Console.WriteLine("OMIM_TXT parse time : " + stopwatch.ElapsedMilliseconds + $" Fields : {records.Count}");
            return records;
        }

        public RecordOmin GetFieldValue(string[] lines, int currentLineIndex, int nextIndex)
        {
            string fieldValue = string.Empty;
            List<int> index = new List<int>();
            for (int i = currentLineIndex + 1; i < nextIndex; i++)
            {
                if (lines[i].StartsWith('*'))
                {
                    index.Add(i);
                }
            }
            string nb = string.Empty;
            string title = string.Empty;
            string clinic = string.Empty;
            for (int i = 0; i<index.Count-1; i++)
            {
                if (lines[index[i]].StartsWith("*FIELD* NO"))
                {
                    nb = getValue(index[i]+1, index[i+1], lines);
                }
                if (lines[index[i]].StartsWith("*FIELD* TI"))
                {
                    title = getValue(index[i] + 1, index[i + 1], lines);

                }
                if (lines[index[i]].StartsWith("*FIELD* CS"))
                {
                    clinic = getValue(index[i] + 1, index[i + 1], lines);
                }
            }

            /*for (int i = index[0]+1; i < index[1]-1 ; i++)
            {
                nb+= lines[i].Trim() + Environment.NewLine;
            }
            for (int i = index[1]; i < index[2]; i++)
            {
                title += lines[i].Trim() + Environment.NewLine;
            }*/
            
            
            return new RecordOmin(nb, title, clinic);
        }

        public string getValue(int min, int max, string[] lines)
        {
            string value = string.Empty;
            for (int i = min; i < max; i++)
            {
                value += lines[i].Trim() + Environment.NewLine;
            }
            return value;
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
