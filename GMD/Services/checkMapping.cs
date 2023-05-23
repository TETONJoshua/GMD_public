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
                OmimTXTClassID.Add(omim.Number);
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

            sameOccurences = drugBankName.Intersect(keggName).Count();
            differences = drugBankName.Except(keggName).Count();

            Console.WriteLine($"Between DrugBank and Kegg there is {sameOccurences} similarities and {differences} more in the XML.\n");

            sameOccurences = keggName.Intersect(drugBankName).Count();
            differences = keggName.Except(drugBankName).Count();

            Console.WriteLine($"Between DrugBank and Kegg there is {sameOccurences} similarities and {differences} more in the keg.\n");

            sameOccurences = keggATC.Intersect(ChemicalATC).Count();
            differences = keggATC.Except(ChemicalATC).Count();

            Console.WriteLine($"Between ChemicalSources and Kegg there is {sameOccurences} similarities and {differences} more in the keg.\n");

            sameOccurences = ChemicalATC.Intersect(keggATC).Count();
            differences = ChemicalATC.Except(keggATC).Count();

            Console.WriteLine($"Between Chemical and Kegg there is {sameOccurences} similarities and {differences} more in the Chemical.\n");

        }
    }

}

