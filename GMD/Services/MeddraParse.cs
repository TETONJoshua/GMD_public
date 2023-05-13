using GMD.Mapping;

namespace GMD.Services
{
    public class MeddraParse
    {
        public List<Meddra> ParseMeddra()
        {
            List<Meddra> symptomList = new List<Meddra>();

            // Séparer les lignes en fonction des sauts de ligne
            string[] lines = File.ReadAllLines("sources/meddra.tsv");

            // Parcourir chaque ligne
            foreach (string line in lines)
            {
                // Séparer les éléments en fonction des tabulations
                string[] elements = line.Trim().Split('\t');

                Meddra entry = new Meddra
                {
                    Code = elements[0],
                    Symptoms = elements[3]
                };

                symptomList.Add(entry);

            }
            return symptomList;
        }
    }
}
