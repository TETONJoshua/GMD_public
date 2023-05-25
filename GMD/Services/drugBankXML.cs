using GMD.Mapping;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System.Diagnostics;
using System.Xml;

namespace GMD.Services
{
    public class drugBankXML
    {
        //Parses the DrugBank.xml file
        public List<RecordDrugBankXML> parseXML()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<RecordDrugBankXML> parsedResult = new List<RecordDrugBankXML>();

            //Native .NET XMl loader, uses the DOM to navigates through datas. Fast and easy to use.
            XmlDocument drugBankSource = new XmlDocument();
            XmlTextReader reader = new XmlTextReader("./sources/drugbank.xml");
            drugBankSource.Load("./sources/drugbank.xml");
            reader.Close();
            XmlNode drugbank = drugBankSource["drugbank"];
            foreach (XmlNode drug in drugbank.ChildNodes)
            {
                RecordDrugBankXML record = new RecordDrugBankXML();
                if (drug["name"] != null) { record.name = drug["name"].InnerText; }
                if (drug["toxicity"] != null) { record.toxicity = drug["toxicity"].InnerText; }
                if (drug["indication"] != null) { record.indication = drug["indication"].InnerText; }
                parsedResult.Add(record);
            }
            stopwatch.Stop();
            Console.WriteLine("XML parse time : " + stopwatch.ElapsedMilliseconds);
            return parsedResult;
        }

        //Indexes all the XML datas
        //TextField allows a parsed query to be performed within the index field. This means that Lucene won't expect a perfect fit and will rank results with a score
        //String field works as a key and Lucene will look for a perfect or almost perfect fit.
        public void indexXmlDatas(List<RecordDrugBankXML> drugBankDatas, IndexWriter writer)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();  
            foreach (RecordDrugBankXML drug in drugBankDatas)
            {
                Document doc = new Document();
                doc.Add(new StringField("drugName_DB", drug.name, Field.Store.YES));
                doc.Add(new TextField("toxicity", drug.toxicity, Field.Store.YES));
                doc.Add(new TextField("indication", drug.indication, Field.Store.YES));              
                writer.AddDocument(doc);
            }
            writer.Commit();
            stopwatch.Stop();
            Console.WriteLine("XML index time : " + stopwatch.ElapsedMilliseconds);   
        }
    }
}
