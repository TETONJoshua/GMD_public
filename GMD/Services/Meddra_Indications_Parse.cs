using GMD.Mapping;

namespace GMD.Services
{
    public class Meddra_Indications_Parse
    {
        public static List<Meddra_Indications> ParseMeddra()
        {
            List<Meddra_Indications> symptomList = new List<Meddra_Indications>();

            // Séparer les lignes en fonction des sauts de ligne
            string[] lines = File.ReadAllLines("sources/meddra_all_indications.tsv");

            // Parcourir chaque ligne
            foreach (string line in lines)
            {
                // Séparer les éléments en fonction des tabulations
                string[] elements = line.Trim().Split('\t');

                Meddra_Indications entry = new Meddra_Indications
                {
                    Code = elements[1],
                    Symptoms = elements[3],
                    CID = elements[0],
                };

                symptomList.Add(entry);

            }
            return symptomList;
        }
    }
}
