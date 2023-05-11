using GMD.Mapping;

namespace GMD.Services
{
    public class ominTXT
    {
        public static void Main(string[] args)
        {
            string filePath = "sources/omin.txt"; // Remplacez "chemin_vers_le_fichier" par le chemin réel du fichier

            List<Record> records = ParseFile(filePath);

            // Affichage des enregistrements extraits
            foreach (Record record in records)
            {
                Console.WriteLine("Number: " + record.Number);
                Console.WriteLine("Title: " + record.Title);
                Console.WriteLine("Clinical Features: " + record.ClinicalFeatures);
                Console.WriteLine();
            }
        }

        public static List<Record> ParseFile(string filePath)
        {
            List<Record> records = new List<Record>();
            Record currentRecord = null;

            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                if (line.StartsWith("*RECORD*"))
                {
                    currentRecord = new Record();
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
