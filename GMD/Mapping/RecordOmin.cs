namespace GMD.Mapping
{
    public class RecordOmin
    {
        public string Number { get; set; }
        public string Title { get; set; }
        public string ClinicalFeatures { get; set; }

        public RecordOmin(string Number ="", string Title="", string ClinicalFeatures="")
        {
            this.Number = Number;
            this.Title = Title;
            this.ClinicalFeatures = ClinicalFeatures;
        }

        public override string ToString()
        {
            return $"Record(Number='{this.Number}', Title='{this.Title}')";
        }
    }
}
