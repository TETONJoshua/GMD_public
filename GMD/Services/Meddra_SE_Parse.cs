using GMD.Mapping;

namespace GMD.Services
{
    public class Meddra_SE_Parse
    {
        public static List<Meddra_SE> ParseMeddra_SE()
        {
            List<Meddra_SE> symptomList = new List<Meddra_SE>();

            // Séparer les lignes en fonction des sauts de ligne
            string[] lines = File.ReadAllLines("sources/meddra_all_se.tsv");

            // Parcourir chaque ligne
            foreach (string line in lines)
            {
                // Séparer les éléments en fonction des tabulations
                string[] elements = line.Trim().Split('\t');

                Meddra_SE entry = new Meddra_SE
                {
                    Code = elements[2],
                    Symptoms = elements[5],
                    CID = elements[0],
                };

                symptomList.Add(entry);

            }
            return symptomList;
        }
    }
}
