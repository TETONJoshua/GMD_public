﻿using GMD.Mapping;
using System.Text.RegularExpressions;

namespace GMD.Services
{
    public class hpo
    {
        public static List<Term> GetTerms()
        {
            List<Term> terms = new();
            Regex term_regex = new(@"\[Term\]\n(.*?)\n\n", RegexOptions.Singleline);
            Regex id_regex = new(@"id: (.*?)\n");
            Regex name_regex = new(@"name: (.*?)\n");
            Regex def_regex = new(@"def: ""(.*?)"" \[.*?\]\n");
            Regex synonym_regex = new(@"synonym: ""(.*?)"" EXACT .*?\n");
            Regex xref_regex = new(@"xref: (.*?)\n");
            Regex is_a_regex = new(@"is_a: (.*?) !.*?\n");

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
                    foreach (Match synonym_match in synonym_regex.Matches(term_text))
                    {
                        term_synonyms.Add(synonym_match.Groups[1].Value);
                    }
                    List<string> term_xrefs = new();
                    foreach (Match xref_match in xref_regex.Matches(term_text))
                    {
                        term_xrefs.Add(xref_match.Groups[1].Value);
                    }
                    List<string> term_is_a = new();
                    foreach (Match is_a_match in is_a_regex.Matches(term_text))
                    {
                        term_is_a.Add(is_a_match.Groups[1].Value);
                    }
                    Term term = new(term_id, term_name, term_def, term_synonyms, term_xrefs, term_is_a);
                    terms.Add(term);
                }
            }
            return terms;
        }
    }
}
