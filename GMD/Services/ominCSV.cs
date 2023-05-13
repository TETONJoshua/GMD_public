using GMD.Mapping;

namespace GMD.Services
{
    public class ominCSV
    {
        public List<RecordOminCSV> ParseCsv()
        {
            List<RecordOminCSV> RecordOminCSVs = new List<RecordOminCSV>();

            using (StreamReader reader = new StreamReader("sources/omim_onto.csv"))
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
                        ClassId = values[0].Replace("http://purl.bioontology.org/ontology/OMIM/",""),
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
