using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MergeRESX.Models
{
    internal class DataFormat
    {
    }






    class Resource
    {
        public string Name { get; }
        public string Value { get; }
        public string Comment { get; }
        public string FileName { get; }
        public XElement Xml { get; }

        public Resource(string name, string value, string fileName, string comment, XElement xml)
        {
            Name = name;
            Value = value;
            Comment = comment;
            FileName = fileName;
            Xml = xml;
        }

        public static Resource Parse(XElement element, string fileName)
        {
            string name = element.Attribute("name")!.Value;
            string value = element.Element("value")!.Value;
            string comment = element.Element("comment")?.Value!;
            return new Resource(name, value, fileName, comment, element);
        }
    }

}
