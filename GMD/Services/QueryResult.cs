namespace GMD.Services
{
    public class QueryResult
    {
        
        string sourceSymptom { get; set; }//symptom that matches the researched symptom

        string definition { get; set; }//the definition of the source symptom

        public List<Disease> foundDiseases { get; set; }
        public List<Drug> foundDrugCause { get; set; }

        public QueryResult(string sourceSymptom, string definition, List<Disease> foundDiseases, List<Drug> drugs)
        {
            this.sourceSymptom = sourceSymptom;
            this.definition = definition;
            this.foundDiseases = foundDiseases;
            this.foundDrugCause = drugs;
        }
    }
    public class Disease
    {
        public string diseaseName { get; set; }

        public List<Drug> cures { get; set; }

        public Disease(string diseaseName, List<Drug> cures) 
        {
            this.diseaseName = diseaseName;
            this.cures = cures;
        }
    }

    public class Drug
    {
        public string drugName { get; set; }
        public string toxicity { get; set;}
        public string indication { get; set;}

    }

}
