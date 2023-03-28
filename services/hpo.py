import re

class Term:
    def __init__(self, term_id=None, name=None, definition=None, synonyms=None, xrefs=None, is_a=None):
        self.term_id = term_id
        self.name = name
        self.definition = definition
        self.synonyms = synonyms or []
        self.xrefs = xrefs or []
        self.is_a = is_a or []

    def __str__(self):
        return f"Term(ID='{self.term_id}', Name='{self.name}')"

def get_terms():
    with open('sources/hpo.obo', 'r') as f:
        terms = []
        term_regex = re.compile(r'\[Term\]\n(.*?)\n\n', re.DOTALL)
        id_regex = re.compile(r'id: (.*?)\n')
        name_regex = re.compile(r'name: (.*?)\n')
        def_regex = re.compile(r'def: "(.*?)" \[.*?\]\n')
        synonym_regex = re.compile(r'synonym: "(.*?)" EXACT .*?\n')
        xref_regex = re.compile(r'xref: (.*?)\n')
        is_a_regex = re.compile(r'is_a: (.*?) !.*?\n')
        for term_match in term_regex.finditer(f.read()):
            term_text = term_match.group(1)
            term_id = id_regex.search(term_text).group(1)
            term_name = name_regex.search(term_text).group(1)
            if(def_regex.search(term_text) is None):
                term_def = "None"
            else:
                term_def = def_regex.search(term_text).group(1)
            term_synonyms = [synonym_match.group(1) for synonym_match in synonym_regex.finditer(term_text)]
            term_xrefs = [xref_match.group(1) for xref_match in xref_regex.finditer(term_text)]
            term_is_a = [is_a_match.group(1) for is_a_match in is_a_regex.finditer(term_text)]
            term = Term(term_id, term_name, term_def, term_synonyms, term_xrefs, term_is_a)
            terms.append(term)
    return terms