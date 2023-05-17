namespace GMD.Mapping
{
    public class RecordDrugBankXML
    {
        public string name { get; set; }

        public string interaction { get; set; }
        public string toxicity { get; set; }
        public string indication { get; set; }
        public List<string> synonyms { get; set; }
        public List<string> products { get; set; }

        public RecordDrugBankXML(string name, string interaction, string toxicity, string indication, List<string> synonyms, List<string> products )
        {
           
            this.name = name;
            this.indication = interaction;
            this.interaction = interaction;
            this.toxicity = toxicity;
            this.synonyms = synonyms;
            this.products = products;
        }
        public RecordDrugBankXML()
        {
            this.name = "";
            this.interaction = "";
            this.toxicity = "";
            this.indication = "";
            this.synonyms = new List<string>();
            this.products = new List<string>();
        }
    }
}
