using Lucene.Net.Documents;

namespace GMD.Mapping
{
    public class RecordBrKEG
    {
        public string kegId { get; set; }
        public string medicName { get; set; }

        public RecordBrKEG(string kegId, string medicName ) 
        {
            this.kegId = kegId;
            this.medicName = medicName;
        }
    }
}
