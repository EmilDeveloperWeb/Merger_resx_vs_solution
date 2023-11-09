using MergeRESX.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MergeRESX.Helpers
{/* -;)  */

    internal class XMLMerger
    {

        public List<XElement> MergeAllNodes(IEnumerable<Resource> resLs)
        {
            List<XElement> xmlNodes = new List<XElement>();

            foreach (var r in resLs)
            {
                var xml = r.Xml;
                var name = xml.Attribute("name")!.Value;
                var value = xml.Element("value")!.Value;

                XElement xe = new XElement("data",
                 new XAttribute("name", name),
                 new XElement("fileName", r.FileName),
                 new XElement("value", value));

                xmlNodes.Add(xe);
            }

            return xmlNodes;
        }


        public List<XElement> AddFileNamesInNodeValueThis(List<XElement> xmls)
        {
            List<XElement> dls = new List<XElement>();

            foreach (var r in xmls)
            {
                var name = r.Attribute("name")!.Value;
                var value = r.Element("value")!.Value;
                var fileName = r.Element("fileName")!.Value;

                if (name == "$this.Text")
                {
                    string strFName = Path.GetFileNameWithoutExtension(fileName);
                    var indx = strFName.LastIndexOf(".");

                    if (indx > 0)
                    {
                        strFName = strFName[..indx] + ".Text";
                    }
                    else
                    {
                        strFName = strFName + ".Text";
                    }

                    name = strFName;
                }

                XElement xe = new XElement("data",
                 new XAttribute("name", name),
                 new XElement("fileName", fileName),
                 new XElement("value", value));

                dls.Add(xe);
            }

            return dls;
        }


        public List<XElement> RemoveNodesWithRepeatedName(List<XElement> xmls, string folderName)
        { 

            List<XElement> dls = new List<XElement>();
          
            string removededNodes = "Those nodes has been removed because Name key repeats\n\n";
          
            List<XElement> idsRepeated = new List<XElement>();


            foreach (var nd in xmls)
            {
                var name = nd.Attribute("name")!.Value;
                var value = nd.Element("value")!.Value;
            
                var has = dls.Where(n => String.Equals(n.Attribute("name")!.Value, name.ToString(),
                    StringComparison.OrdinalIgnoreCase)).Any();
            
            
                if (!has) { dls.Add(nd); }
                else {
            
                    var sameIdValueIsNew = idsRepeated.Where(n => String.Equals(n.Element("value")!.Value, value.ToString(),
                       StringComparison.OrdinalIgnoreCase)).Any();
            
                    var hasStt = dls.Where(e=>e.Attribute("name")!.Value == name && e.Element("value")!.Value == value).Any() ; 
            
            
                    // if value is diff for this id i add 
                    if (!sameIdValueIsNew && !hasStt) { 
                       
                       idsRepeated.Add(nd); 
                       var infoNodeRemoved = $"Name: {name}, Value:{value} \n";
                       removededNodes += infoNodeRemoved;
                    
                    }
                }
            
            }


          Console.Write(removededNodes);
          Console.Write("---------------------\n\n");

          File.WriteAllText(folderName + "/List_RemovedNodes.txt", removededNodes);


          return dls;

        }


        public XDocument FormatFile(List<XElement> output)
        {


            // get only needed nodes
            var output2 = output.Select(x => 
            new XElement("data", 
               new XAttribute("name", x.Attribute("name")!.Value),
               new XElement("value", x.Element("value")!.Value))
            ).ToList();


            // IDK what it is but it comes with the files .resx and do not work without it...
            XElement schema = XElement.Parse(@"<xsd:schema id='root' xmlns='' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'>
                <xsd:import namespace='http://www.w3.org/XML/1998/namespace' />
                <xsd:element name='root' msdata:IsDataSet='true'>
                  <xsd:complexType>
                    <xsd:choice maxOccurs='unbounded'>
                      <xsd:element name='metadata'>
                        <xsd:complexType>
                          <xsd:sequence>
                            <xsd:element name='value' type='xsd:string' minOccurs='0' />
                          </xsd:sequence>
                          <xsd:attribute name='name' use='required' type='xsd:string' />
                          <xsd:attribute name='type' type='xsd:string' />
                          <xsd:attribute name='mimetype' type='xsd:string' />
                          <xsd:attribute ref='xml:space' />
                        </xsd:complexType>
                      </xsd:element>
                      <xsd:element name='assembly'>
                        <xsd:complexType>
                          <xsd:attribute name='alias' type='xsd:string' />
                          <xsd:attribute name='name' type='xsd:string' />
                        </xsd:complexType>
                      </xsd:element>
                      <xsd:element name='data'>
                        <xsd:complexType>
                          <xsd:sequence>
                            <xsd:element name='value' type='xsd:string' minOccurs='0' msdata:Ordinal='1' />
                            <xsd:element name='comment' type='xsd:string' minOccurs='0' msdata:Ordinal='2' />
                          </xsd:sequence>
                          <xsd:attribute name='name' type='xsd:string' use='required' msdata:Ordinal='1' />
                          <xsd:attribute name='type' type='xsd:string' msdata:Ordinal='3' />
                          <xsd:attribute name='mimetype' type='xsd:string' msdata:Ordinal='4' />
                          <xsd:attribute ref='xml:space' />
                        </xsd:complexType>
                      </xsd:element>
                      <xsd:element name='resheader'>
                        <xsd:complexType>
                          <xsd:sequence>
                            <xsd:element name='value' type='xsd:string' minOccurs='0' msdata:Ordinal='1' />
                          </xsd:sequence>
                          <xsd:attribute name='name' type='xsd:string' use='required'/>
                        </xsd:complexType>
                      </xsd:element>
                    </xsd:choice>
                  </xsd:complexType>
                </xsd:element>
            </xsd:schema>");

            // without it the file do not works
            XElement resheader1 = new XElement("resheader",
                new XAttribute("name", "resmimetype"),
                new XElement("value", "text/microsoft-resx"));

            XElement resheader2 = new XElement("resheader",
                new XAttribute("name", "version"),
                new XElement("value", "2.0"));

            XElement resheader3 = new XElement("resheader",
                new XAttribute("name", "reader"),
                new XElement("value", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));

            XElement resheader4 = new XElement("resheader",
                new XAttribute("name", "writer"),
                    new XElement("value", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));


              XDocument? formatedReady = new XDocument();
           
            try
            {

                var resOutput = new XDocument(new XElement("root", output));

                var newElements = new[] { schema, resheader1, resheader2, resheader3, resheader4 }.Concat(resOutput.Root!.Elements());

                formatedReady = new XDocument(new XElement("root", newElements));


                Console.WriteLine("Base format elements inserted.");

            }
            catch (Exception e)
            {
                Console.WriteLine("Error incerting base format els...: " + e.Message);
            }

               return formatedReady;

            }


        public List<XElement> DetermineRepeatedValues(List<XElement> resouses, string folderDestination)
        {
            List<XElement> lNodes = new List<XElement>();

            try { 

                var xml = XDocument.Load(folderDestination + "/List_RepForValues.xml");

                foreach (var n in xml.Root!.Elements())
                {

                    var name = n.Attribute("name")!.Value;
                    var value = n.Element("value")!.Value;


                    XElement xe = new XElement("data",
                     new XAttribute("name", name),
                       new XElement("value", value));


                    lNodes.Add(xe);

                }


            } catch { }


            var duplicateNodes = resouses
            .GroupBy(x => x.Element("value")!.Value)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g).ToList();

            
            string infoStr = "Those values repeat for different keys:\n\n";
            List<XElement> repNodes = new List<XElement>();

            if (duplicateNodes.Count == 0)
            {
                infoStr = "No values repeat for different keys.";
            }
            
            foreach (var nd in duplicateNodes!)
            {
                var text = $"Name: {nd.Attribute("name")!.Value} Value: {nd.Element("value")!.Value}\n";
                infoStr += text;
                repNodes.Add(nd);

            }

            Console.Write(infoStr);
            Console.Write("There will be a file that contains the values repeated but for different key without repeating...\n");
            Console.Write("\n-------------------\n");
             
            var ll = lNodes.Concat(repNodes);

            var uniqueValues = ll.GroupBy(n => new 
            { Nombre = n.Attribute("name")!.Value, 
                Valor = n.Element("value")!.Value })
                .Select(g => g.First())
                .ToList();



            /*
               si existiera la carpeta _lang i en ella se encontrase otro
               List_RepForValues.xml
               al hacer el merge la app va a mergear tambien esta lista
               no va a repetir valores

               if exists _lang dir and it contains any List_RepForValues.xml
               when merge the app will merge the list with the file as well
               what is needed when sevral merges are required

             */



            var v = "";

            foreach(var av in uniqueValues)
            {
                v += "Name:"+ av.Attribute("name")!.Value +" Vlue:"+ av.Element("value")!.Value+"\n";
            }
            File.WriteAllText(folderDestination + "/List_RepForValues.txt", v);
            
            var fileNodeRep = new XDocument(new XElement("root", uniqueValues));
            fileNodeRep.Save(folderDestination + "/List_RepForValues.xml");

            return duplicateNodes;
        }

     
        public List<XElement> RemoveOthersThanTextNodes(List<XElement> output)
        {
            Console.Write("Nodes without text will be removed as the method is on... \n\n");

             var ntxt = output.Where(x=>x.Attribute("name")!.Value.EndsWith(".Text")).ToList();

            // new nodes only text nodes
            return ntxt;
        }


        public List<XElement> AddFileNameToRepeatedIds(List<XElement> xmls)
        {
            List<XElement> dls = new List<XElement>();

            foreach (var nd in xmls)
            {
                var name = nd.Attribute("name")!.Value;
                var value = nd.Element("value")!.Value;
                var fileName = nd.Element("fileName")!.Value;


                var has = dls.Where(n => String.Equals(n.Attribute("name")!.Value, name.ToString(),
                    StringComparison.OrdinalIgnoreCase)).Any();


                if (!has) { dls.Add(nd); }
                else
                {

                    string strFName = Path.GetFileNameWithoutExtension(fileName);
                    var indx = strFName.LastIndexOf(".");

                    if (indx > 0)
                    {
                        strFName = strFName[..indx];// + ".Text";
                    }

                    name = strFName + "." + name;// + ".Text";

                    XElement xe = new XElement("data",
                     new XAttribute("name", name),
                     new XElement("fileName", fileName),
                     new XElement("value", value));

                    dls.Add(xe);
                }

            }

            return dls;
        }


        public List<XElement> RemoveRepeatedNodes(List<XElement> xmls)
        {

            List<XElement> dls = new List<XElement>();

            foreach (var nd in xmls)
            {
                var name = nd.Attribute("name")!.Value;
                var value = nd.Element("value")!.Value;

                var has = dls.Where(n => string.Equals(n.Attribute("name")!.Value, name.ToString(),
                   StringComparison.OrdinalIgnoreCase)
                   && string.Equals(n.Element("value")!.Value, value.ToString(),
                     StringComparison.OrdinalIgnoreCase)).Any();
                 
                if (!has) { dls.Add(nd); }

            }

            return dls;
        }

    }

}
