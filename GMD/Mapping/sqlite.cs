namespace GMD.Mapping
{
    public class sqlite
    {
        public string disease_db { get; set; }
        public string disease_id { get; set; }
        public string disease_label { get; set; }

        public string diseaseFreq { get; set; }
        public List<string> synonyms { get; set; }

        public sqlite(List<string> synonyms, string disease_db="", string disease_id = "", string disease_label = "", string diseaseFreq ="")
        {
            this.disease_db = disease_db;
            this.disease_id = disease_id;
            this.disease_label = disease_label;
            this.diseaseFreq = diseaseFreq;
            this.synonyms = synonyms;
        }
    }
}
