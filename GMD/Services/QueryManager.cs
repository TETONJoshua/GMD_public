﻿using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace GMD.Services
{
    public static class QueryManager
    {
        /*Retrieve all molecules linked to given symptoms*/
        public static List<DrugResult> getMoleculesFromSymptoms(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {

            QueryParser parser = new QueryParser(luceneVersion, "toxicity", standardAnalyzer);
            parser.DefaultOperator = QueryParser.AND_OPERATOR;
            Query query = parser.Parse(symptom);
            TopDocs topDocs = searcher.Search(query, n: 1);
            int i;
            List<DrugResult> toxicDrugs = new List<DrugResult>();
            List<DrugResult> toxicDrugRes = new List<DrugResult>();
            for (i = 0; i < topDocs.ScoreDocs.Length; i++)
            {

                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string foundName = resultDoc.Get("drugName_DB");
                string tox = resultDoc.Get("toxicity");
                List<Drug> drugs = new List<Drug>();
                Drug drugT = new Drug(foundName, tox, "");
                drugT.drugScore = topDocs.ScoreDocs[i].Score;
                drugT.sourceDoc = "DRUGBANK";
                drugs.Add(drugT);
                toxicDrugs.Add(new DrugResult("Unknown", drugs));

            }
            toxicDrugs.AddRange(getCIDfromSE(standardAnalyzer, searcher, symptom, luceneVersion));

            return toxicDrugs;
        }
        /*Retrieve all CID linked to given side effect*/
        public static List<DrugResult> getCIDfromSE(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {
            QueryParser parser = new QueryParser(luceneVersion, "name_SE", standardAnalyzer);
            parser.DefaultOperator = QueryParser.AND_OPERATOR;
            Query query = parser.Parse(symptom);
            TopDocs topDocs = searcher.Search(query, n: 10000);
            int i;
            string CID_SE;
            List<string> CID_SEs = new List<string>();
            List<DrugResult> drugResults = new List<DrugResult>();
            for (i = 0; i < topDocs.ScoreDocs.Length; i++)
            {

                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                CID_SE = resultDoc.Get("CID_SE");
                bool known = false;
                if (CID_SE != "" && CID_SE != null && topDocs.ScoreDocs[i].Score > 1)
                {
                    foreach (string alreadyKnown in CID_SEs)
                    {
                        if (alreadyKnown == CID_SE)
                        {
                            known = true;
                            break;
                        }
                    }
                    if (!known)
                    {
                        CID_SEs.Add(CID_SE);

                        drugResults.Add(new DrugResult("", getATCFromCIDForSE(searcher, CID_SE, topDocs.ScoreDocs[i].Score).DistinctBy(x => x.drugName).ToList()));
                    }
                }
            }
            return drugResults;
        }

        /*Retrieve all ATC Code linked to given CID for a side effect*/
        public static List<Drug> getATCFromCIDForSE(IndexSearcher searcher, string CID, float score)
        {
            string atcCode;
            Query query = new TermQuery(new Term("CID_CHEM", CID));
            TopDocs topDocs = searcher.Search(query, n: 1);
            List<Drug> drugs = new List<Drug>();
            List<string> atcCodes = new List<string>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                bool known = false;
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                atcCode = resultDoc.Get("ATC_CHEM");
                foreach (string atc in atcCodes)
                {
                    if (atc == atcCode) known = true; break;
                }
                if (atcCode != "" && atcCode != null && !known)
                {
                    drugs.AddRange(getNameFromAtcForSE(searcher, atcCode, score));
                }
            }
            return drugs;
        }
        /*Retrieve all molecules names in the .keg linked to given ATC for a side effect*/
        public static List<Drug> getNameFromAtcForSE(IndexSearcher searcher, string atcCode, float score)
        {
            Query query = new TermQuery(new Term("ATC_KEG", atcCode));
            TopDocs topDocs = searcher.Search(query, n: 10);
            List<Drug> drugs = new List<Drug>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string name = resultDoc.Get("drugName_KEG");
                if (name != "" && name != null)
                {
                    string tox = "";

                    Query query2 = new TermQuery(new Term("drugName_DB", name));
                    TopDocs topDocs2 = searcher.Search(query2, n: 2);
                    for (int j = 0; i < topDocs2.ScoreDocs.Length; i++)
                    {
                        Document resultDoc2 = searcher.Doc(topDocs2.ScoreDocs[j].Doc);
                        string toxicity = resultDoc2.Get("toxicity");
                        if (toxicity != "" && toxicity != null)
                        {
                            tox = toxicity;
                        }
                    }
                    var drugS = new Drug(name, tox, "");
                    drugS.drugScore = score;
                    drugS.sourceDoc = "MEDDRA_SE";
                    drugs.Add(drugS);
                }
            }
            return drugs;
        }

        /*Retrieve all molecules names linked to given ATC Code*/
        public static List<Drug> getNameFromAtc(IndexSearcher searcher, string atcCode, LuceneVersion luceneVersion)
        {
            Query query = new TermQuery(new Term("ATC_KEG", atcCode));
            TopDocs topDocs = searcher.Search(query, n: 1);
            string name;
            List<Drug> cures = new List<Drug>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                name = resultDoc.Get("drugName_KEG");
                if (name != "" && name != null)
                {
                    cures.AddRange(getIndicationFromName(searcher, name, luceneVersion));
                    foreach (Drug cure in cures)
                    {
                        cure.sourceDoc = "MEDDRA_IND";
                        cure.drugScore = topDocs.ScoreDocs[i].Score;
                    }
                }
            }
            return cures;
        }

        /*Retrieve all indications linked to given drug*/
        public static List<Drug> getIndicationFromName(IndexSearcher searcher, string name, LuceneVersion luceneVersion)
        {
            string indic;

            Query query = new TermQuery(new Term("drugName_DB", name));
            TopDocs topDocs = searcher.Search(query, n: 1);
            List<Drug> cures = new List<Drug>();
            Drug cure = new Drug(name);
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                indic = resultDoc.Get("indication");

                if (indic != "" && indic != null)
                {
                    cure.indication = indic;
                }
            }
            cures.Add(cure);
            return cures;
        }

        /*Retrieve all CID linked to given symptoms*/
        public static List<string> getCIDFromSymptom(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {
            QueryParser parser = new QueryParser(luceneVersion, "symptoms", standardAnalyzer);
            Query query = parser.Parse(symptom);
            TopDocs topDocs = searcher.Search(query, n: 100000);
            string CID;
            List<string> CIDs = new List<string>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                CID = resultDoc.Get("CID");
                bool known = false;
                if (CID != "" && CID != null)
                {
                    foreach (string alreadyKnown in CIDs)
                    {
                        if (alreadyKnown == CID)
                        {
                            known = true;
                            break;
                        }
                    }
                    if (!known)
                    {
                        CIDs.Add(CID);
                    }
                }
            }
            return CIDs;
        }

        /*Retrieve all CID linked to given CUI in Meddra_Indic*/
        public static List<Drug> getCIDFromCUI_INDIC(IndexSearcher searcher, string CUI, LuceneVersion luceneVersion, float score)
        {
            string CID;
            Query query = new TermQuery(new Term("CUI_IND", CUI));
            TopDocs topDocs = searcher.Search(query, n: 1000);
            List<string> CIDs = new List<string>();
            List<Drug> cures = new List<Drug>();
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                CID = resultDoc.Get("CID_IND");

                if (CID != "" && CID != null)
                {
                    bool known = false;
                    if (CID != "" && CID != null)
                    {
                        foreach (string alreadyKnown in CIDs)
                        {
                            if (alreadyKnown == CID)
                            {
                                known = true;
                                break;
                            }
                        }
                        if (!known)
                        {

                            var curesS = getATCFromCID(searcher, CID, luceneVersion);

                            if (curesS.Count != 0)
                            {
                                CIDs.Add(CID);
                                cures.AddRange(curesS);
                            }
                        }
                    }
                }
            }
            return cures;
        }

        /*Retrieve the result of the given query*/
        public static QueryResult getQueryResult(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {
            return new QueryResult(getDiseasesFromSymptom(standardAnalyzer, searcher, symptom, luceneVersion), getMoleculesFromSymptoms(standardAnalyzer, searcher, symptom, luceneVersion));
        }
        
        //Gets all the diseases results with the associated symptoms that matched with the input, and the drugs that can cure the disease if there are any.
        public static List<DiseaseResult> getDiseasesFromSymptom(Analyzer standardAnalyzer, IndexSearcher searcher, string symptom, LuceneVersion luceneVersion)
        {
            QueryParser parser = new QueryParser(luceneVersion, "symptoms_HPO", standardAnalyzer);
            Query query = parser.Parse(symptom);
            TopDocs topDocs = searcher.Search(query, n: 20);
            string HP;
            List<string> HPs = new List<string>();
            List<DiseaseResult> allDiseasesResults = new List<DiseaseResult>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                HP = resultDoc.Get("HP_HPO");

                string symptoms = resultDoc.Get("symptoms_HPO");
                string definition = resultDoc.Get("definition");
                bool known = false;
                List<Disease> diseases = new List<Disease>();
                List<Drug> symptomTreatments = new List<Drug>();

                if (HP != "" && HP != null)
                {
                    foreach (string alreadyKnown in HPs)
                    {
                        if (alreadyKnown == HP)
                        {
                            known = true;

                            break;
                        }
                    }
                    if (!known)
                    {
                        diseases.AddRange(getNamesFromHP(searcher, HP, luceneVersion, topDocs.ScoreDocs[i].Score, standardAnalyzer));


                        if (diseases.Count != 0)
                        {
                            foreach (Disease d in diseases)
                            {
                                d.cures = getTreatmentsForDisease(searcher, d.diseaseName, luceneVersion, standardAnalyzer);
                            }
                        }
                        HPs.Add(HP);
                        List<string> UMLSs = getUMLSFromHP(searcher, HP);

                        foreach (string UMLS in UMLSs)
                        {
                            symptomTreatments.AddRange(getCIDFromCUI_INDIC(searcher, UMLS, luceneVersion, topDocs.ScoreDocs[i].Score));
                        }
                        symptomTreatments.AddRange(getTreatmentsForDisease(searcher, symptom, luceneVersion, standardAnalyzer));


                        allDiseasesResults.Add(new DiseaseResult(symptoms, topDocs.ScoreDocs[i].Score, diseases, symptomTreatments));
                    }
                }
            }
            allDiseasesResults.AddRange(getOmimFromSymptoms(searcher, symptom, luceneVersion, standardAnalyzer));
            allDiseasesResults.Add(new DiseaseResult(symptom, 0, new List<Disease>(), getTreatmentsForSymptom(searcher, symptom, luceneVersion, standardAnalyzer)));
            return allDiseasesResults;
        }

        /*Retrieve all UMLS linked to given HP*/
        public static List<string> getUMLSFromHP(IndexSearcher searcher, string HP)
        {
            Query query = new TermQuery(new Term("HP_HPO", HP));
            TopDocs topDocs = searcher.Search(query, n: 1000);
            List<string> UMLSs = new List<string>();
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string UMLS = resultDoc.Get("CUI_HPO");
                if (UMLS != null & UMLS != "")
                {
                    UMLSs.Add(UMLS);
                }
            }


            return UMLSs;
        }

        /*Retrieve all diseases name linked to given HP*/
        public static List<Disease> getNamesFromHP(IndexSearcher searcher, string HP, LuceneVersion luceneVersion, float score, Analyzer standardAnalyzer)
        {
            Query query = new TermQuery(new Term("HP_SQL", HP));
            TopDocs topDocs = searcher.Search(query, n: 10000000);
            Dictionary<string, int> names = new Dictionary<string, int>();
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string name = resultDoc.Get("name_SQL");
                int freq = 7;
                bool known = false;
                string strFreq = resultDoc.Get("diseaseFrequency");

                if (strFreq != null && strFreq != "None")
                {
                    freq = Int32.Parse(strFreq);
                }
                if (name != null && name != "")
                {
                    foreach (var nameKnown in names)
                    {
                        if (name.Contains(nameKnown.Key))
                        {
                            known = true;
                        }
                    }
                    if (!known)
                    {
                        names.Add(name, freq);
                    }

                }
            }
            var sortednames = names.OrderBy(x => x.Value);
            List<Disease> returnedDiseases = new List<Disease>();
            foreach (var name in sortednames)
            {
                returnedDiseases.Add(new Disease(name.Key, name.Value, "HPO_ANNOT"));
            }
            return returnedDiseases;
        }

        /*Retrieve all disease name linked to given UMLS*/
        public static List<Disease> getTitleFromUMLS(IndexSearcher searcher, string UMLS, LuceneVersion luceneVersion, Analyzer standardAnalyzer)
        {
            Query query = new TermQuery(new Term("CUI_onto", UMLS));
            TopDocs topDocs = searcher.Search(query, n: 1000);
            List<string> names = new List<string>();
            List<Disease> diseases = new List<Disease>();
            List<string> titleOmim = new List<string>();
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {

                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string strDisease = resultDoc.Get("name_onto");

                if (strDisease != null && strDisease != "")
                {
                    names.Add(strDisease);
                    diseases.Add(new Disease(strDisease, "OMIM_ONTO"));
                }
            }

            return diseases;
        }

        /*Retrieve all genetics diseases linked to given symptoms*/
        public static List<DiseaseResult> getOmimFromSymptoms(IndexSearcher searcher, string symptoms, LuceneVersion luceneVersion, Analyzer standardAnalyzer)
        {
            List<DiseaseResult> diseaseResults = new List<DiseaseResult>();
            QueryParser parser = new QueryParser(luceneVersion, "symptomsOmim", standardAnalyzer);
            parser.DefaultOperator = QueryParser.AND_OPERATOR;
            Query query = parser.Parse(symptoms);

            TopDocs topDocs = searcher.Search(query, n: 1000);
            List<string> names = new List<string>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                bool known = false;
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string sym = resultDoc.Get("symptomsOmim");

                string title = resultDoc.Get("name_TXT");
                foreach (string name in names)
                {
                    if (name == title) { known = true; break; }
                }

                if (title != "" && title != null && !known)
                {
                    names.Add(title);
                    List<Disease> diseases = new List<Disease>();
                    Disease dis = new Disease(title, "OMIM_TXT");
                    dis.cures = getTreatmentsForDisease(searcher, title, luceneVersion, standardAnalyzer);
                    diseases.Add(dis);
                    diseaseResults.Add(new DiseaseResult(sym, topDocs.ScoreDocs[i].Score, diseases, getTreatmentsForDisease(searcher, symptoms, luceneVersion, standardAnalyzer)));
                }
            }


            return diseaseResults;
        }

        /*Retrieve all possibles treaments linked to given symptoms*/
        public static List<Drug> getTreatmentsForSymptom(IndexSearcher searcher, string diseaseName, LuceneVersion luceneVersion, Analyzer standardAnalyzer)
        {

            QueryParser parser = new QueryParser(luceneVersion, "name_IND", standardAnalyzer);
            Query query = parser.Parse(diseaseName);
            TopDocs topDocs = searcher.Search(query, n: 5);
            List<Drug> suggestedDrugs = new List<Drug>();

            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                string CID = resultDoc.Get("CID_IND");
                if (CID == null || CID == "")
                {
                    string CUI = resultDoc.Get("CUI_IND_MED");

                    List<string> CIDs = new List<string>();
                    if (CUI != null && CUI != "")
                    {
                        Query query2 = new TermQuery(new Term("CUI_IND", CUI));
                        TopDocs topDocs2 = searcher.Search(query2, n: 1000);

                        for (int j = 0; j < topDocs2.ScoreDocs.Length; j++)
                        {
                            Document resultDoc2 = searcher.Doc(topDocs2.ScoreDocs[j].Doc);
                            string CIDres = resultDoc2.Get("CID_IND");
                            if (CIDres != null && CIDres != "")
                            {
                                CIDs.Add(CIDres);
                            }
                        }
                    }
                    foreach (string CIDres in CIDs)
                    {
                        suggestedDrugs.AddRange(getATCFromCID(searcher, CIDres, luceneVersion));
                    }
                }
                else
                {
                    suggestedDrugs.AddRange(getATCFromCID(searcher, CID, luceneVersion));
                }
            }
            return suggestedDrugs;
        }

        /*Retrieve all molecules linked to given disease*/
        public static List<Drug> getTreatmentsForDisease(IndexSearcher searcher, string diseaseName, LuceneVersion luceneVersion, Analyzer standardAnalyzer)
        {
            try
            {
                diseaseName = diseaseName.ToLower().Replace("disease", "");
                diseaseName = diseaseName.ToLower().Replace("syndrome", "");
                diseaseName = diseaseName.ToLower().Replace("type", "");
                diseaseName = diseaseName.ToLower().Replace("0", "");
                diseaseName = diseaseName.ToLower().Replace("1", "");
                diseaseName = diseaseName.ToLower().Replace("2", "");
                diseaseName = diseaseName.ToLower().Replace("3", "");
                diseaseName = diseaseName.ToLower().Replace("4", "");
                diseaseName = diseaseName.ToLower().Replace("5", "");
                diseaseName = diseaseName.ToLower().Replace("6", "");
                diseaseName = diseaseName.ToLower().Replace("7", "");
                diseaseName = diseaseName.ToLower().Replace("8", "");
                diseaseName = diseaseName.ToLower().Replace("9", "");
                diseaseName = diseaseName.ToLower().Replace("#", "");
                diseaseName = diseaseName.ToLower().Replace("%", "");

                QueryParser parser = new QueryParser(luceneVersion, "indication", standardAnalyzer);
                Query query = parser.Parse(diseaseName);
                TopDocs topDocs = searcher.Search(query, n: 10);
                List<Drug> suggestedDrugs = new List<Drug>();


                for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
                {

                    Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                    string drugName = resultDoc.Get("drugName_DB");
                    string indic = resultDoc.Get("indication");

                    if (drugName != null && drugName != "")
                    {

                        Drug drug = new Drug(drugName, indic);
                        drug.sourceDoc = "DRUGBANK";
                        drug.drugScore = topDocs.ScoreDocs[i].Score;
                        suggestedDrugs.Add(drug);
                    }
                }
                return suggestedDrugs.DistinctBy(x => x.drugName).ToList();
            }
            catch
            {
                return new List<Drug>();
            }
        }

        /*Retrieve all ATC Code linked to given CID*/
        public static List<Drug> getATCFromCID(IndexSearcher searcher, string CID, LuceneVersion luceneVersion)
        {
            string atcCode;
            Query query = new TermQuery(new Term("CID_CHEM", CID));
            TopDocs topDocs = searcher.Search(query, n: 1);
            List<Drug> cures = new List<Drug>();
            for (int i = 0; i < topDocs.ScoreDocs.Length; i++)
            {
                Document resultDoc = searcher.Doc(topDocs.ScoreDocs[i].Doc);
                atcCode = resultDoc.Get("ATC_CHEM");
                if (atcCode != "" && atcCode != null)
                {
                    cures.AddRange(getNameFromAtc(searcher, atcCode, luceneVersion));
                }
            }
            return cures;
        }


    }
}
