namespace GMD.Mapping
{
    public class Meddra_Indications
    {
        public string CUI{ get; set; }
        public string CID { get; set; }
        public string Symptom { get; set; }


        public Meddra_Indications(string Code = "", string Symptom = "", string cID = "")
        {
            this.CUI = Code;
            this.Symptom = Symptom;
            this.CID = cID;
        }
    }
}
