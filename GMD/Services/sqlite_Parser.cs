using GMD.Mapping;

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
    }
}
