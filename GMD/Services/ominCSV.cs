using GMD.Mapping;

namespace GMD.Services
{
    public class ominCSV
    {
        public static List<RecordOminCSV> ParseCsv(string filePath)
        {
            List<RecordOminCSV> RecordOminCSVs = new List<RecordOminCSV>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                bool isFirstLine = true;

                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue; // Ignore la première ligne (en-têtes des colonnes)
                    }

                    string[] values = line.Split(',');

                    RecordOminCSV RecordOminCSV = new RecordOminCSV
                    {
                        ClassId = values[0],
                        Cui = values[5],
                        Synonyms = values[2],
                        PreferredLabel = values[1]
                    };

                    RecordOminCSVs.Add(RecordOminCSV);
                }
            }

            return RecordOminCSVs;
        }
    }
}
