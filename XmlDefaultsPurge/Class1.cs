using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using System.Xml.XPath;

namespace XmlDefaultsPurge
{
	public class DefaultsXmlProcessorFactory
	{

		public XmlDocument DefaultsDoc;
		public void LoadDefaultsDoc(string filename)
		{
			System.Xml.XmlDocument xmldoc = new XmlDocument();
			xmldoc.Load(filename);

			this.DefaultsDoc = xmldoc;
		}

		private bool _saveBackup = true;
		public bool SaveBackup { get { return this._saveBackup; } set { _saveBackup = value; } }

		public void Process(string filename)
		{
			DefaultsXmlProcessor processor = new DefaultsXmlProcessor();
			processor.DefaultsDoc = DefaultsDoc;

			processor.Execute(filename);
		}

	}

	internal class DefaultsXmlProcessor
	{

		public XmlDocument DefaultsDoc;

		public XmlReader TargetRead;
		public XmlWriter TargetWrite;

		private Stack<XElement> _Stack;

		private static System.Text.RegularExpressions.Regex S_ArrayIndex = new System.Text.RegularExpressions.Regex(@"\[([0-9]+)\]");

		public void Execute(string filename)
		{
			using(TargetRead = new XmlTextReader(filename))
			{
				using(TargetWrite = new System.Xml.XmlTextWriter(Console.Out))
				{
					_ExecuteInternal(TargetRead, TargetWrite);
				}
			}
		}
		private void _ExecuteInternal(XmlReader TargetRead, XmlWriter TargetWrite)
		{
			while(TargetRead.Read())
			{
				
				switch(TargetRead.NodeType)
				{
					case XmlNodeType.Element:
						
						elementTree.Add(TargetRead.Name);
						
						TargetWrite.WriteStartElement(TargetRead.Prefix, TargetRead.LocalName, TargetRead.NamespaceURI);
						
						bool emptied = _ProcessElement(TargetRead, TargetWrite);

						if(TargetRead.IsEmptyElement || emptied) 
						{ 
							TargetWrite.WriteEndElement(); 
							elementTree.RemoveAt(elementTree.Count - 1); 
						}

						break;

					case XmlNodeType.EndElement:
						TargetWrite.WriteEndElement();
						if(elementTree.Count>0) elementTree.RemoveAt(elementTree.Count - 1);
						break;

					case XmlNodeType.Whitespace:
						TargetWrite.WriteWhitespace(TargetRead.Value);
						break;

					default:
						TargetWrite.WriteNode(TargetRead, true);
						break;
				}
			}
		}
		private List<string> elementTree = new List<string>();

		public Func<string,string,int> Compare = (v1,v2)=>{return string.Compare(v1,v2,StringComparison.InvariantCulture);};

		private bool _ProcessElement(XmlReader TargetRead, XmlWriter TargetWrite)
		{
			if(TargetRead.HasAttributes)
			{

				string elementXPath = string.Join("/", elementTree);
				var defaultsElement = DefaultsDoc.SelectSingleNode(elementXPath) as XmlElement;

				if(defaultsElement == null)
				{
					if(TargetRead.MoveToFirstAttribute())
					{
						TargetWrite.WriteAttributes(TargetRead, true);
						TargetRead.MoveToElement();
					}
					return false;
				}

				int attributesWritten = 0;
				while(TargetRead.MoveToNextAttribute())
				{
					bool writeAttribute = true;

					var defaultsAttribute = defaultsElement.Attributes.GetNamedItem(TargetRead.LocalName, TargetRead.NamespaceURI) as XmlAttribute;
					if(defaultsAttribute != null)
					{
						if(Compare(TargetRead.Value, defaultsAttribute.Value) == 0)
						{
							//write value since its different from a default value.
							writeAttribute = false;
						}
					}
					
					if(writeAttribute)
					{
						TargetWrite.WriteAttributeString(TargetRead.Prefix, TargetRead.LocalName, TargetRead.NamespaceURI, TargetRead.Value);
						attributesWritten++;
					}
				}

				return (attributesWritten == 0);
			}

			return false;
		}


	}
}
