namespace GMD.Services
{
    public class QueryResult
    {
        

        public List<DiseaseResult> foundDiseases { get; set; }
        public List<DrugResult> foundDrugCause { get; set; }

        public QueryResult(List<DiseaseResult> foundDiseases, List<DrugResult> drugs)
        {
           
            this.foundDiseases = foundDiseases;
            this.foundDrugCause = drugs;
        }
        public QueryResult(List<DiseaseResult> foundDiseases)
        {

            this.foundDiseases = foundDiseases;
        }
    }

    public class DrugResult
    {
        public string frequence { get; set; }
        
        public List<Drug> drugs { get; set; }

        public DrugResult (string frequence, List<Drug> drugs)
        {
            this.frequence = frequence;
            this.drugs = drugs;
        }
    }

    public class DiseaseResult
    {
        public string matchingSymtom;
        public float symptomScore { get; set; }
        public List<Drug> symptomCures { get; set; }
        public List<Disease> diseases { get; set; }

        public DiseaseResult(string matchingSymtom, float symptomScore, List<Disease> diseases, List<Drug> symptomCures)
        {
            this.matchingSymtom = matchingSymtom;
            this.symptomScore = symptomScore;
            this.diseases = diseases;
            this.symptomCures = symptomCures;
        }
    }
    public class Disease
    {
        public float score { get; set; }
        public string diseaseName { get; set; }
        public int diseaseFrequency { get; set; }
        public List<string> synonyms { get; set; }
        public List<Drug> cures { get; set; }
        public Disease(string diseaseName, int freq, List<string> synonyms, List<Drug> cures) 
        {
            this.diseaseName = diseaseName;
            this.diseaseFrequency = freq;
            this.synonyms = synonyms;
            this.cures = cures;           
        }
        public Disease(string diseaseName)
        {
            this.diseaseName = diseaseName;
            diseaseFrequency = 7;
            this.synonyms = new List<string>();
            this.cures = new List<Drug>();
        }
        public Disease(string diseaseName, int diseaseFreq)
        {
            this.diseaseName = diseaseName;
            this.diseaseFrequency= diseaseFreq;
            this.synonyms = new List<string>();
            this.cures = new List<Drug>();
        }
    }

    public class Drug
    {
        public string drugName { get; set; }
        public string toxicity { get; set;}
        public string indication { get; set;}

        public float drugScore { get; set; }

        public Drug(string drugName, string toxicity, string indication)
        {
            this.drugName = drugName;
            this.toxicity = toxicity;
            this.indication = indication;
        }

        public Drug(string drugName, string indication)
        {
            this.drugName = drugName;
            this.indication = indication;
            this.toxicity = "";
        }
        public Drug(string drugName)
        {
            this.drugName = drugName;
            this.indication = "";
            this.toxicity = "";
        }
    }

}
