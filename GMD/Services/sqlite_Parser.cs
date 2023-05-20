using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;

namespace GMD.Services
{
    public class sqlite_Parser
    {
        public List<sqlite> ParseSqlite()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<sqlite> list = new List<sqlite>();
            using (StreamReader reader = new StreamReader("sources/sqlite.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] data = line.Split(", ");
                    if (data.Length >= 3)
                    {
                        string diseaseFreq = data[8];
                        switch (diseaseFreq)
                        {
                            case "HP:0040280":
                                diseaseFreq = "0";
                                break;
                            case "HP:0040281":
                                diseaseFreq = "1";
                                break;
                            case "HP:0040282":
                                diseaseFreq = "2";
                                break;
                            case "HP:0040283":
                                diseaseFreq = "3";
                                break;
                            case "HP:0040284":
                                diseaseFreq = "4";
                                break;
                            case "HP:0040285":
                                diseaseFreq = "5";
                                break;
                            default:
                                break;
                        }


                        sqlite entry = new sqlite
                        {
                            disease_db = data[0],
                            disease_id = data[4],
                            disease_label = data[2],
                            diseaseFreq = diseaseFreq,
                        };

                        list.Add(entry);
                    }
                }
            }
            stopwatch.Stop();
            Console.WriteLine("SQLITE parse time : " + stopwatch.ElapsedMilliseconds);
            list.DistinctBy(x => x.disease_db).ToList();
            return list;
        }

        public void indexSqliteDatas(List<sqlite> SqliteDatas, IndexWriter writer)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (sqlite drug in SqliteDatas)
            {
                Document doc = new Document();
                doc.Add(new TextField("name", drug.disease_label, Field.Store.YES));
                
                doc.Add(new StringField("HP", drug.disease_id.Trim(), Field.Store.YES));
                doc.Add(new StringField("db", drug.disease_db, Field.Store.YES));
                doc.Add(new StringField("diseaseFrequency", drug.diseaseFreq, Field.Store.YES));
                writer.AddDocument(doc);
            }

            writer.Commit();
            stopwatch.Stop();
            Console.WriteLine("SQLITE index time : " + stopwatch.ElapsedMilliseconds);
        }
    }
}
