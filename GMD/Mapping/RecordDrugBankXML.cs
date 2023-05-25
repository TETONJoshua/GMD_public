namespace GMD.Mapping
{
    public class RecordDrugBankXML
    {
        public string name { get; set; }
        public string toxicity { get; set; }
        public string indication { get; set; }


        public RecordDrugBankXML(string name, string interaction, string toxicity)
        {
           
            this.name = name;
            this.indication = interaction;
            this.toxicity = toxicity;

        }
        public RecordDrugBankXML()
        {
            this.name = "";
            this.toxicity = "";
            this.indication = "";
        }
    }
}
