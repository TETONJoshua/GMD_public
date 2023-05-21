namespace GMD.Mapping
{
    public class RecordOmin
    {
        public string Number { get; set; }
        public string Title { get; set; }
        public List<string> ClinicalFeatures { get; set; }

        public RecordOmin(List<string> ClinicalFeatures, string Number ="", string Title="")
        {
            this.Number = Number;
            this.Title = Title;
            this.ClinicalFeatures = ClinicalFeatures;
        }
    }
}
