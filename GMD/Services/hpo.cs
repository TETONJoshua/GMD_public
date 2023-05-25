using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace GMD.Services
{
    public class hpo
    {
        //Parses the HOP.obo file
        public List<RecordHPO> ParseHpo()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<RecordHPO> terms = new();
            Regex term_regex = new(@"\[Term\]\n(.*?)\n\n", RegexOptions.Singleline);
            Regex id_regex = new(@"id: (.*?)\n");
            Regex name_regex = new(@"name: (.*?)\n");
            Regex def_regex = new(@"def: ""(.*?)"" \[.*?\]\n");
            Regex synonym_regex = new(@"synonym: ""(.*?)"" EXACT .*?\n");
            Regex xref_regex = new(@"xref: UMLS:(.*?)\n");
            Regex is_a_regex = new(@"is_a: HP:(.*?)\n");

            using (StreamReader sr = new StreamReader(@"sources/hpo.obo"))
            {
                string file_contents = sr.ReadToEnd();
                foreach (Match term_match in term_regex.Matches(file_contents))
                {
                    string term_text = term_match.Groups[1].Value;
                    string term_id = id_regex.Match(term_text).Groups[1].Value;
                    string term_name = name_regex.Match(term_text).Groups[1].Value;
                    string term_def = def_regex.Match(term_text).Success ? def_regex.Match(term_text).Groups[1].Value : "None";
                    List<string> term_synonyms = new();
                    //Get synonyms
                    foreach (Match synonym_match in synonym_regex.Matches(term_text))
                    {
                        term_synonyms.Add(synonym_match.Groups[1].Value);
                    }
                    List<string> term_xrefs = new();
                    //get UMLS of symptoms
                    foreach (Match xref_match in xref_regex.Matches(term_text))
                    {
                        term_xrefs.Add(xref_match.Groups[1].Value);
                    }
                    List<string> term_is_a = new();
                    //Get symtpom
                    foreach (Match isa_match in is_a_regex.Matches(term_text))
                    {
                        term_is_a.Add(isa_match.Groups[1].Value);
                    }
                    RecordHPO term = new(term_id, term_name, term_def, term_synonyms, term_xrefs, term_is_a);
                    terms.Add(term);
                }
            }
            stopwatch.Stop();
            Console.WriteLine("HPO_OBO parse tim : " + stopwatch.ElapsedMilliseconds);
            return terms;
        }

        //Indexes all the HPO.obo datas
        public void indexHPODatas(List<RecordHPO> HPODatas, IndexWriter writer)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (RecordHPO data in HPODatas)
            {
                if(data.xrefs.Count > 0 )
                {

                    Document doc = new Document();
                    doc.Add(new StringField("HP_HPO", data.term_id, Field.Store.YES));
                    doc.Add(new TextField("definition", data.definition, Field.Store.YES));
                    foreach(string xref in data.xrefs) 
                    {
                        doc.Add(new StringField("CUI_HPO", xref , Field.Store.YES));
                    }                    
                    string turboSyno = "";

                    //indexes synonyms with symptom names so the research can be performed on synonyms too 
                    foreach (string synonym in data.synonyms)
                    {
                        turboSyno += synonym + " ; ";
                    }
                    turboSyno += data.name + " ; ";
                    doc.Add(new TextField("symptoms_HPO", turboSyno, Field.Store.YES));
                    writer.AddDocument(doc);
                }
               
            }

            writer.Commit();
            stopwatch.Stop();
            Console.WriteLine("HPO index time : " + stopwatch.ElapsedMilliseconds);
        }

    }
}
