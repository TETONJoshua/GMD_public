namespace GMD.Mapping
{
    public class RecordOminCSV
    {
        public string ClassId { get; set; }
        public string Cui { get; set; }
        public string Synonyms { get; set; }
        public string PreferredLabel { get; set; }

        public RecordOminCSV(string ClassId = "", string Cui = "", string Synonyms = "", string PrefferedLabel ="")
        {
            this.ClassId = ClassId;
            this.Cui = Cui;
            this.Synonyms = Synonyms;
            this.PreferredLabel = PrefferedLabel;
        }

        public override string ToString()
        {
            return $"RecordOminCSV(Number='{this.ClassId}', Title='{this.PreferredLabel}')";
        }
    }
}
