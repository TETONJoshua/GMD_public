namespace GMD.Mapping
{
    public class RecordDrugBankXML
    {
        public string atcCode { get; set; } 
        public string name { get; set; }

        public string interaction { get; set; }
        public string toxicity { get; set; }
        public List<string> synonyms { get; set; }
        public List<string> products { get; set; }

        public RecordDrugBankXML(string atcCode, string name, string interaction, string toxicity, List<string> synonyms, List<string> products )
        {
            this.atcCode = atcCode;
            this.name = name;
            this.interaction = interaction;
            this.synonyms = synonyms;
            this.products = products;
        }
        public RecordDrugBankXML()
        {
            this.name = "";
            this.interaction = "";
            this.toxicity = "";
            this.synonyms = new List<string>();
            this.products = new List<string>();
        }
    }
}
