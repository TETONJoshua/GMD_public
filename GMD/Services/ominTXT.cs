using GMD.Mapping;
using J2N.Numerics;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;
using System.Xml.Linq;

namespace GMD.Services
{
    public class ominTXT
    {

        public List<RecordOmin> ParseFile()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<RecordOmin> records = new List<RecordOmin>();
            List<int> index= new List<int>();
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
            for (int i=0; i<index.Count-1;  i++)
            { 
                records.Add(GetFieldValue(lines, index[i], index[i + 1]));               
            }
            stopwatch.Stop();
            Console.WriteLine("OMIM_TXT parse time : " + stopwatch.ElapsedMilliseconds + $" Fields : {records.Count}");
            return records;
        }

        public RecordOmin GetFieldValue(string[] lines, int currentLineIndex, int nextIndex)
        {
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
           List<string> clinic = new List<string>();

            for (int i = 0; i<index.Count-1; i++)
            {
                if (lines[index[i]].StartsWith("*FIELD* NO"))
                {
                    nb = getValue(index[i]+1, index[i+1], lines);
                }
                if (lines[index[i]].StartsWith("*FIELD* TI"))
                {
                    title = getValue(index[i] + 1, index[i + 1], lines).ToLower();
                    string[] tst = title.Split(";;");
                    if (tst.Length > 0)
                    {
                        
                        title = tst[0].ToLower();
                    }
                    

                }
                if (lines[index[i]].StartsWith("*FIELD* CS"))
                {
                    clinic = getValueCS(index[i] + 1, index[i + 1], lines);
                }
            }
            
            return new RecordOmin(clinic, nb, title);
        }

        public string getValue(int min, int max, string[] lines)
        {
            string value = "";
            for (int i = min; i < max; i++)
            {
               value += lines[i].Trim() + Environment.NewLine;
               
            }

            return value;
        }
        public List<string> getValueCS(int min, int max, string[] lines)
        {
            List<string> value = new List<string>();
            for (int i = min; i < max; i++)
            {
                var line = lines[i];
                if (line.StartsWith("   ") && !line.StartsWith("   [") && !line.ToLower().Contains("autosomal"))
                {
                    //Console.WriteLine(line);
                    value.Add(line.Replace("   ", ""));
                }
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
                doc.Add(new TextField("name_TXT", drug.Title, Field.Store.YES));
                doc.Add(new StringField("classID", drug.Number.Replace("\n","").Trim(), Field.Store.YES));
                foreach (string line in drug.ClinicalFeatures)
                {
                    doc.Add(new TextField("symptomsOmim", line.Replace(";",""), Field.Store.YES));
                }
                writer.AddDocument(doc);
            }

            writer.Commit();
            stopwatch.Stop();
            Console.WriteLine("OMIM_TXT index time : " + stopwatch.ElapsedMilliseconds);
        }
    }
}
