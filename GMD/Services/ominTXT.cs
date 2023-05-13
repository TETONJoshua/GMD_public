using GMD.Mapping;

namespace GMD.Services
{
    public class ominTXT
    {

        public static List<RecordOmin> ParseFile()
        {
            List<RecordOmin> records = new List<RecordOmin>();
            RecordOmin currentRecord = null;

            string[] lines = File.ReadAllLines("sources/omim.txt");

            foreach (string line in lines)
            {
                if (line.StartsWith("*RECORD*"))
                {
                    currentRecord = new RecordOmin();
                    records.Add(currentRecord);
                }
                else if (line.StartsWith("*FIELD* NO"))
                {
                    currentRecord.Number = GetFieldValue(lines, line);
                }
                else if (line.StartsWith("*FIELD* TI"))
                {
                    currentRecord.Title = GetFieldValue(lines, line);
                }
                else if (line.StartsWith("*FIELD* CS"))
                {
                    currentRecord.ClinicalFeatures = GetFieldValue(lines, line);
                }
            }

            return records;
        }

        public static string GetFieldValue(string[] lines, string currentLine)
        {
            int currentIndex = Array.IndexOf(lines, currentLine);
            string fieldValue = string.Empty;

            for (int i = currentIndex + 1; i < lines.Length; i++)
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
    }
}
