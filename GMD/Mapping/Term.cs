namespace GMD.Mapping
{
    public class Term
    {
        public string term_id{get; set;}
        public string name { get; set; }
        public string definition { get; set; }
        public List<string> synonyms { get; set; }
        public List<string> xrefs { get; set; }
        public List<string> is_a { get; set; }

        public Term(string term_id = "", string name = "", string definition = "", List<string>? synonyms = null, List<string>? xrefs = null, List<string>? is_a = null)
        {
            this.term_id = term_id;
            this.name = name;
            this.definition = definition;
            this.synonyms = synonyms ?? new List<string>();
            this.xrefs = xrefs ?? new List<string>();
            this.is_a = is_a ?? new List<string>();
        }

        public override string ToString()
        {
            return $"Term(ID='{this.term_id}', Name='{this.name}')";
        }
    }
}
