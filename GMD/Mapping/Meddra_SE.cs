namespace GMD.Mapping
{
    public class Meddra_SE
    {
        public string Code { get; set; }
        public string Symptoms { get; set; }

        public string CID { get; set; }

        public Meddra_SE(string Code = "", string Symptom = "", string cID = "")
        {
            this.Code = Code;
            this.Symptoms = Symptom;
            this.CID = cID;
        }
    }
}
