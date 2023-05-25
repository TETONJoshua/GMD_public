using GMD.Mapping;
using System.Linq;

namespace GMD.Services
{
    public class checkMapping
    {
            public void MappingQuality(List<RecordHPO> hpoDatas,List<RecordBrKEG> keggDatas, List<RecordDrugBankXML> drugBankDatas, List<Chemical> chemicalsDatas, List<RecordOmin> ominTxtDatas, List<RecordOminCSV> ominCsvDatas, List<Meddra> meddraDatas, List<sqlite> sqlitesDatas, List<Meddra_freq> meddraFreqDatas, List<Meddra_SE> meddraSeDatas, List<Meddra_Indications> meddraIndicationsData)
            {

            List<String> keggATC = new List<String>();
            List<String> keggName = new List<String>();
            List<String> drugBankName = new List<String>();
            List<String> ChemicalATC = new List<String>();
            List<String> ChemicalCID = new List<String>();
            List<String> meddraCUI = new List<String>();
            List<String> meddraSECUI = new List<String>();
            List<String> meddraSECID = new List<String>();
            List<String> meddraFreqCUI = new List<String>();
            List<String> meddraFreqCID = new List<String>();
            List<String> meddraIndicCUI = new List<String>();
            List<String> meddraIndicCID = new List<String>();
            List<String> HpoUMLS = new List<String>();
            List<String> HpoHP = new List<String>();
            List<String> OmimCSVClassID = new List<String>();
            List<String> OmimCSVUMLS = new List<String>();
            List<String> SqliteHP = new List<String>();
            List<String> OmimTXTClassID = new List<String>();

            foreach(RecordOmin omim in ominTxtDatas)
            {
                OmimTXTClassID.Add(omim.Number.Replace("\n","").Trim());
            }

            foreach (RecordOminCSV omim in ominCsvDatas)
            {
               OmimCSVClassID.Add(omim.ClassId);
               OmimCSVUMLS.Add(omim.Cui);
            }

            foreach (sqlite sqlite in sqlitesDatas)
            {
                SqliteHP.Add(sqlite.disease_id);
            }

            foreach (RecordHPO hpo in hpoDatas)
            {
                HpoHP.Add(hpo.term_id);
                HpoUMLS.AddRange(hpo.xrefs);
            }

            foreach(Meddra med in meddraDatas)
            {
                meddraCUI.Add(med.Code);
            }

            foreach (Meddra_SE medSE in meddraSeDatas)
            {
                meddraSECUI.Add(medSE.Code);
                meddraSECID.Add(medSE.CID);
            }

            foreach (Meddra_freq medFreq in meddraFreqDatas)
            {
                meddraFreqCUI.Add(medFreq.Code);
                meddraFreqCID.Add(medFreq.CID);

            }

            foreach (Meddra_Indications medIndic in meddraIndicationsData)
            {
                meddraIndicCUI.Add(medIndic.CUI);
                meddraIndicCID.Add(medIndic.CID);

            }

            foreach (Chemical chem in chemicalsDatas)
            {
                ChemicalATC.Add(chem.ATC);
                ChemicalCID.Add(chem.CID);
            }

            foreach (RecordBrKEG kegg in keggDatas)
            {
                keggATC.Add(kegg.ATC);
                keggName.Add(kegg.medicName);
            }

            foreach(RecordDrugBankXML drug in drugBankDatas)
            {
                drugBankName.Add(drug.name);
            }

            int sameOccurences = 0;
            int differences = 0;

            //Check Drugbank keg link
            sameOccurences = drugBankName.Intersect(keggName).Count();
            differences = drugBankName.Except(keggName).Count();

            Console.WriteLine($"Between DrugBank and Kegg there is {sameOccurences} similarities and {differences} more in the XML.");

            sameOccurences = keggName.Intersect(drugBankName).Count();
            differences = keggName.Except(drugBankName).Count();

            Console.WriteLine($"Between DrugBank and Kegg there is {sameOccurences} similarities and {differences} more in the keg.");

            //Check keg Chemical Sources link
            sameOccurences = keggATC.Intersect(ChemicalATC).Count();
            differences = keggATC.Except(ChemicalATC).Count();

            Console.WriteLine($"Between ChemicalSources and Kegg there is {sameOccurences} similarities and {differences} more in the keg.");

            sameOccurences = ChemicalATC.Intersect(keggATC).Count();
            differences = ChemicalATC.Except(keggATC).Count();

            Console.WriteLine($"Between Chemical and Kegg there is {sameOccurences} similarities and {differences} more in the Chemical.");

            //Check Chemical Sources Meddra files link
            sameOccurences = ChemicalCID.Intersect(meddraIndicCID).Count();
            differences = ChemicalCID.Except(meddraIndicCID).Count();

            Console.WriteLine($"Between ChemicalSources and meddra_all_indications there is {sameOccurences} similarities and {differences} more in Chemical.");

            sameOccurences = meddraIndicCID.Intersect(ChemicalCID).Count();
            differences = meddraIndicCID.Except(ChemicalCID).Count();

            Console.WriteLine($"Between ChemicalSources and meddra_all_indications there is {sameOccurences} similarities and {differences} more in Meddra_all_indications.");


            sameOccurences = ChemicalCID.Intersect(meddraFreqCID).Count();
            differences = ChemicalCID.Except(meddraFreqCID).Count();

            Console.WriteLine($"Between ChemicalSources and meddra_all_freq there is {sameOccurences} similarities and {differences} more in Chemical.");

            sameOccurences = meddraFreqCID.Intersect(ChemicalCID).Count();
            differences = meddraFreqCID.Except(ChemicalCID).Count();

            Console.WriteLine($"Between ChemicalSources and meddra_all_freq there is {sameOccurences} similarities and {differences} more in Meddra_all_freq.");

            sameOccurences = ChemicalCID.Intersect(meddraSECID).Count();
            differences = ChemicalCID.Except(meddraSECID).Count();

            Console.WriteLine($"Between ChemicalSources and meddra_all_se there is {sameOccurences} similarities and {differences} more in Chemical");

            sameOccurences = meddraSECID.Intersect(ChemicalCID).Count();
            differences = meddraSECID.Except(ChemicalCID).Count();

            Console.WriteLine($"Between ChemicalSources and meddra_all_se there is {sameOccurences} similarities and {differences} more in Meddra_all_se.");

            //Check Hpo Meddra files link
            sameOccurences = HpoUMLS.Intersect(meddraIndicCUI).Count();
            differences = HpoUMLS.Except(meddraIndicCUI).Count();

            Console.WriteLine($"Between Hpo and meddra_all_indications there is {sameOccurences} similarities and {differences} more in Hpo.");

            sameOccurences = meddraIndicCUI.Intersect(HpoUMLS).Count();
            differences = meddraIndicCUI.Except(HpoUMLS).Count();

            Console.WriteLine($"Between Hpo and meddra_all_indications there is {sameOccurences} similarities and {differences} more in Meddra_all_indications.");


            sameOccurences = HpoUMLS.Intersect(meddraFreqCUI).Count();
            differences = HpoUMLS.Except(meddraFreqCUI).Count();

            Console.WriteLine($"Between Hpo and meddra_all_freq there is {sameOccurences} similarities and {differences} more in Hpo.");

            sameOccurences = meddraFreqCUI.Intersect(HpoUMLS).Count();
            differences = meddraFreqCUI.Except(HpoUMLS).Count();

            Console.WriteLine($"Between Hpo and meddra_all_freq there is {sameOccurences} similarities and {differences} more in Meddra_all_freq.");

            sameOccurences = HpoUMLS.Intersect(meddraSECUI).Count();
            differences = HpoUMLS.Except(meddraSECUI).Count();

            Console.WriteLine($"Between Hpo and meddra_all_se there is {sameOccurences} similarities and {differences} more in Hpo.");

            sameOccurences = meddraSECUI.Intersect(HpoUMLS).Count();
            differences = meddraSECUI.Except(HpoUMLS).Count();

            Console.WriteLine($"Between Hpo and meddra_all_se there is {sameOccurences} similarities and {differences} more in Meddra_all_se.");

            sameOccurences = HpoUMLS.Intersect(meddraCUI).Count();
            differences = HpoUMLS.Except(meddraCUI).Count();

            Console.WriteLine($"Between Hpo and meddra there is {sameOccurences} similarities and {differences} more in Hpo.");

            sameOccurences = meddraCUI.Intersect(HpoUMLS).Count();
            differences = meddraCUI.Except(HpoUMLS).Count();

            Console.WriteLine($"Between Hpo and meddra there is {sameOccurences} similarities and {differences} more in Meddra.");

            //Check Hpo Hpo_annotations link
            sameOccurences = HpoHP.Intersect(SqliteHP).Count();
            differences = HpoHP.Except(SqliteHP).Count();

            Console.WriteLine($"Between Hpo and hpo_annotations is {sameOccurences} similarities and {differences} more in Hpo.");

            sameOccurences = SqliteHP.Intersect(HpoHP).Count();
            differences = SqliteHP.Except(HpoHP).Count();

            Console.WriteLine($"Between Hpo and hpo_annotations there is {sameOccurences} similarities and {differences} more in hpo_annotations.");

            //Check hpo OmimCSV links
            sameOccurences = HpoUMLS.Intersect(OmimCSVUMLS).Count();
            differences = HpoUMLS.Except(OmimCSVUMLS).Count();

            Console.WriteLine($"Between Hpo and omim_onto there is {sameOccurences} similarities and {differences} more in Hpo.");

            sameOccurences = OmimCSVUMLS.Intersect(HpoUMLS).Count();
            differences = OmimCSVUMLS.Except(HpoUMLS).Count();

            Console.WriteLine($"Between Hpo and omim_onto there is {sameOccurences} similarities and {differences} more in omim_onto.");

            //Check omim_onto omimTxt link
            sameOccurences = OmimCSVClassID.Intersect(OmimTXTClassID).Count();
            differences = OmimCSVClassID.Except(OmimTXTClassID).Count();

            Console.WriteLine($"Between omimTXT and omim_onto there is {sameOccurences} similarities and {differences} more in omim_onto.");

            sameOccurences = OmimTXTClassID.Intersect(OmimCSVClassID).Count();
            differences = OmimTXTClassID.Except(OmimCSVClassID).Count();

            Console.WriteLine($"Between omimTXT and omim_onto there is {sameOccurences} similarities and {differences} more in omimTxt.\n");
        }
    }

}

