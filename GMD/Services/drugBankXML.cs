using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;
using System.Xml;

namespace GMD.Services
{
    public class drugBankXML
    {
        public int countNullATC, countRecord, countNullNames, countNullTox, countNullInt;
        public List<RecordDrugBankXML> parseXML()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
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
                if (drug["interaction"] != null) { record.interaction = drug["interaction"].InnerText; }
                if (drug["indication"] != null) { record.indication = drug["indication"].InnerText; }
                else { countNullInt++; }
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
               
                parsedResult.Add(record);
               

            }
            stopwatch.Stop();
            Console.WriteLine("XML parse time : " + stopwatch.ElapsedMilliseconds);
            return parsedResult;
        }

        public void indexXmlDatas(List<RecordDrugBankXML> drugBankDatas, IndexWriter writer)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();  
            foreach (RecordDrugBankXML drug in drugBankDatas)
            {
                Document doc = new Document();
                doc.Add(new StringField("drugName", drug.name, Field.Store.YES));
                doc.Add(new TextField("toxicity", drug.toxicity, Field.Store.YES));
                doc.Add(new TextField("interaction", drug.interaction, Field.Store.YES));
                doc.Add(new TextField("indication", drug.indication, Field.Store.YES));
                foreach (string product in drug.products)
                {
                    doc.Add(new StringField("product", product, Field.Store.YES));
                }
                foreach (string synonym in drug.synonyms)
                {
                    doc.Add(new StringField("synonym", synonym, Field.Store.YES));
                }
                writer.AddDocument(doc);
            }
            writer.Commit();
            stopwatch.Stop();
            Console.WriteLine("XML index time : " + stopwatch.ElapsedMilliseconds);   
        }
    }
}
