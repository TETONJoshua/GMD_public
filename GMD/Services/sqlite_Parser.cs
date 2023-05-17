﻿using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace GMD.Services
{
    public class sqlite_Parser
    {
        public List<sqlite> ParseSqlite()
        {
            List<sqlite> list = new List<sqlite>();
            using (StreamReader reader = new StreamReader("sources/sqlite.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] data = line.Split(',');
                    if (data.Length >= 3)
                    {
                        sqlite entry = new sqlite
                        {
                            disease_db = data[0],
                            disease_id = data[1],
                            disease_label = data[2]
                        };

                        list.Add(entry);
                    }
                }
            }
            return list;
        }

        public void indexSqliteDatas(List<sqlite> SqliteDatas, IndexWriter writer)
        {

            foreach (sqlite drug in SqliteDatas)
            {
                Document doc = new Document();
                doc.Add(new StringField("title", drug.disease_label, Field.Store.YES));
                doc.Add(new StringField("HP", drug.disease_id, Field.Store.YES));
                doc.Add(new StringField("db", drug.disease_db, Field.Store.YES));
                writer.AddDocument(doc);
            }

            writer.Commit();
        }
    }
}
