namespace GMD.Mapping
{
    public class Meddra_freq
    {
        public string Code { get; set; }
        public string Symptoms { get; set; }

        public string CID { get; set; }

        public string freq { get; set; }

        public Meddra_freq(string Code = "", string Symptom = "", string cID = "", string freq = "")
        {
            this.Code = Code;
            this.Symptoms = Symptom;
            this.CID = cID;
            this.freq = freq;
        }
    }
}
