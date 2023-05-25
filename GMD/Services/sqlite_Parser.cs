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
        //Parses HPO_ANNOTATIONS.sqlite file
        public List<sqlite> ParseSqlite()
        {
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
                        catch {}
                        //uses the HPO codes to replace the frequency with a readable value for the app
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

        //Indexes the annotations datas
        //TextField allows a parsed query to be performed within the index field. This means that Lucene won't expect a perfect fit and will rank results with a score
        //String field works as a key and Lucene will look for a perfect or almost perfect fit.

        public void indexSqliteDatas(List<sqlite> SqliteDatas, IndexWriter writer)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (sqlite drug in SqliteDatas)
            {
                Document doc = new Document();               
                doc.Add(new TextField("name_SQL", drug.disease_label, Field.Store.YES));
                doc.Add(new StringField("HP_SQL", drug.disease_id.Trim(), Field.Store.YES));
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
