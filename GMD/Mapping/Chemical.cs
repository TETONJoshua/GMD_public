namespace GMD.Mapping
{
    public class Chemical
    {
        public string CID { get; set; }
        public string ATC { get; set; }

        public Chemical(string CID = "", string ATC = "") { 

            this.CID = CID;

            this.ATC = ATC; 
        }
    }
}
