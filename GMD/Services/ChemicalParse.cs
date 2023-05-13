using GMD.Mapping;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace GMD.Services
{
    public class ChemicalParse
    {
        public static List<Chemical> ParseChemical()
        {
            List<Chemical> chemicals = new List<Chemical>();
            string[] lines = File.ReadAllLines("sources/chemical.sources.v5.0.tsv");
            foreach (string line in lines)
            {
                if (line.Contains("CIDm") && line.Contains("CIDs") && line.Contains("ATC"))
                {
                    string[] parts = line.Split('\t');

                    if (parts.Length == 4)
                    {
                        // Création d'un objet DataEntry et ajout à la liste
                        Chemical entry = new Chemical
                        {
                            Chemical_Value = parts[0],
                            Alias = parts[1],
                            Source = parts[2] + " " + parts[3],
                        };

                        chemicals.Add(entry);
                    }
                }
            }
            return chemicals;
        }
    }
}
