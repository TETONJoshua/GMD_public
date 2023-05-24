using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using static Lucene.Net.Search.FieldValueHitQueue;

namespace GMD.Services
{
    public class sqlite_Parser
    {
        public List<sqlite> ParseSqlite()
        {
            //parseSqlite();
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<sqlite> list = new List<sqlite>();

            using (var connection = new SqliteConnection("Data Source=sources/hpo_annotations.sqlite"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                    @"
                        SELECT disease_db, sign_id, disease_label, col_9
                        FROM phenotype_annotation
                    ";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string diseaseFreq ="";
                        string diseaseId ="";
                        string diseaseLabel = "", diseaseName="";
                        List<string> synonyms = new List<string>();

                        try {diseaseFreq = reader.GetString(3);}
                        catch (Exception e){}
                        try { diseaseId = reader.GetString(1);}
                        catch (Exception e) { }
                        try
                        {
                            diseaseLabel = reader.GetString(2);
                            string[] fract = diseaseLabel.Split(";;");
                            if (fract[0].Split(", ").Length > 0)
                            {
                                
                                diseaseName = fract[0];
                                
                            }
                            else
                            {
                                diseaseName = diseaseLabel;
                            }
                           
                            foreach (string syn in fract)
                            {
                                synonyms.Add(syn);
                            }

                        }
                        catch { Console.WriteLine("pd"); }
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
                                diseaseFreq = "7";
                                break;
                        }                       
                        list.Add(new sqlite(synonyms, reader.GetString(0), diseaseId, diseaseName.ToLower(), diseaseFreq));
                    }
                }

                stopwatch.Stop();
                Console.WriteLine("SQLITE parse time : " + stopwatch.ElapsedMilliseconds);
                return list;
            }
        }

        public void indexSqliteDatas(List<sqlite> SqliteDatas, IndexWriter writer)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (sqlite drug in SqliteDatas)
            {
                //Console.WriteLine("CONNARD");
                Document doc = new Document();
                /*if (drug.disease_id.Contains("HP:0001423") )
                {
                    Console.WriteLine(drug.disease_label);
                }*/
                doc.Add(new TextField("name_SQL", drug.disease_label, Field.Store.YES));
                doc.Add(new StringField("HP_SQL", drug.disease_id.Trim(), Field.Store.YES));
                doc.Add(new StringField("db", drug.disease_db, Field.Store.YES));
                //Console.WriteLine(drug.disease_label);
                doc.Add(new StringField("diseaseFrequency", drug.diseaseFreq, Field.Store.YES));
                writer.AddDocument(doc);
            }

            writer.Commit();
            stopwatch.Stop();
            Console.WriteLine("SQLITE index time : " + stopwatch.ElapsedMilliseconds);
        }

        public void parseSqlite()
        {
            using (var connection = new SqliteConnection("Data Source=sources/hpo_annotations.sqlite"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                    @"
                        SELECT disease_label
                        FROM phenotype_annotation
                    ";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetString(0);

                        //Console.WriteLine($"Hello, {name}!");
                    }
                }
            }
        }
    }
}
