using Lucene.Net.Documents;

namespace GMD.Mapping
{
    public class RecordBrKEG
    {
        public string ATC { get; set; }
        public string medicName { get; set; }

        public RecordBrKEG(string kegId, string medicName ) 
        {
            this.ATC = kegId;
            this.medicName = medicName;
        }
    }
}
