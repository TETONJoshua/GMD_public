using GMD.Mapping;

namespace GMD.Services
{
    public class Meddra_Freq_Parse
    {
        public List<Meddra_freq> ParseMeddra()
        {
            List<Meddra_freq> symptomList = new List<Meddra_freq>();

            // Séparer les lignes en fonction des sauts de ligne
            string[] lines = File.ReadAllLines("sources/meddra_freq.tsv");

            // Parcourir chaque ligne
            foreach (string line in lines)
            {
                // Séparer les éléments en fonction des tabulations
                string[] elements = line.Trim().Split('\t');

                Meddra_freq entry = new Meddra_freq
                {
                    Code = elements[2],
                    Symptoms = elements[9],
                    CID = elements[0],
                    freq = elements[5],
                };

                symptomList.Add(entry);

            }
            return symptomList;
        }
    }
}
