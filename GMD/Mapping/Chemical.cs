namespace GMD.Mapping
{
    public class Chemical
    {
        public string Chemical_Value { get; set; }
        public string Alias { get; set; }
        public string Source { get; set; }

        public Chemical(string Chemical ="", string Alias = "", string Source ="") { 
            this.Chemical_Value = Chemical;
            this.Alias = Alias;
            this.Source = Source;
        }
    }
}
