namespace GMD.Mapping
{
    public class Meddra_Indications
    {
        public string Code { get; set; }
        public string Symptoms { get; set; }

        public string CID { get; set; }

        public Meddra_Indications(string Code = "", string Symptom = "", string cID = "")
        {
            this.Code = Code;
            this.Symptoms = Symptom;
            this.CID = cID;
        }
    }
}
