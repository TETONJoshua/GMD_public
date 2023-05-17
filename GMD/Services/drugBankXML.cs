using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Xml;

namespace GMD.Services
{
    public class drugBankXML
    {
        public int countNullATC, countRecord, countNullNames, countNullTox;
        public List<RecordDrugBankXML> parseXML()
        {

            List<RecordDrugBankXML> parsedResult = new List<RecordDrugBankXML>();
            XmlDocument drugBankSource = new XmlDocument();
            XmlTextReader reader = new XmlTextReader("./sources/drugbank.xml");
            drugBankSource.Load("./sources/drugbank.xml");
            reader.Close();
            XmlNode drugbank = drugBankSource["drugbank"];
            Console.WriteLine(drugBankSource.LastChild.Name);
            foreach (XmlNode drug in drugbank.ChildNodes)
            {
                //Console.WriteLine(drug["atc-codes"].FirstChild.Name);
                countRecord++;
                RecordDrugBankXML record = new RecordDrugBankXML();
                if (drug["name"] != null) { record.name = drug["name"].InnerText; }
                else { countNullNames++; }
                if (drug["toxicity"] != null) { record.toxicity = drug["toxicity"].InnerText; }
                else { countNullTox++; }
                if (drug["synonyms"] != null)
                {
                    foreach (XmlNode synonym in drug["synonyms"])
                    {
                        record.synonyms.Add(synonym.InnerText);
                    }
                }
                if (drug["products"] != null)
                {
                    foreach (XmlNode product in drug["products"])
                    {
                        if (product["name"] != null)
                        {
                            record.products.Add(product["name"].InnerText);
                        }
                    }

                }
                //Console.WriteLine(record.name + " : " + record.products.Count);
                /*if (drug["interaction"]!= null)
                {
                    record.interaction = drug["interaction"].InnerText;
                }*/

                parsedResult.Add(record);


            }
            return parsedResult;


        }

        public void indexXmlDatas(List<RecordDrugBankXML> drugBankDatas, IndexWriter writer)
        {

            foreach (RecordDrugBankXML drug in drugBankDatas)
            {
                Document doc = new Document();
                doc.Add(new StringField("name", drug.name, Field.Store.YES));
                doc.Add(new StringField("toxicity", drug.toxicity, Field.Store.YES));
                doc.Add(new StringField("interaction", drug.interaction, Field.Store.YES));
                foreach (string product in drug.products)
                {
                    doc.Add(new StringField("product", product, Field.Store.YES));
                }
                foreach (string synonym in drug.synonyms)
                {
                    doc.Add(new StringField("Synonym", synonym, Field.Store.YES));
                }
                writer.AddDocument(doc);
            }

            writer.Commit();
        }


    }
}
