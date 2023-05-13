namespace GMD.Mapping
{
    public class Meddra
    {
        public string Code { get; set; }
        public string Symptoms { get; set; }

        public Meddra(string Code = "", string Symptom = "")
        {
            this.Code = Code;
            this.Symptoms = Symptom;
        }
    }
}
