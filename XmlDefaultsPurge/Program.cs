using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlDefaultsPurge
{
	class Program
	{
		static void Main(string[] args)
		{
			DefaultsXmlProcessorFactory factory = new DefaultsXmlProcessorFactory();
			factory.LoadDefaultsDoc("C:\\Projects\\NationalGrid\\system.serviceModel.defaults.xml");

			string[] filenamesToProcess = { 
				@"C:\Projects\NationalGrid\nationalgridretailweb\Source\US Retail Web UI\USRetailWebApp\web.config", 
				@"C:\Projects\NationalGrid\nationalgridretailweb\Source\US Retail Web UI\USRetailWebApp\web.Development7.config", 
				@"C:\Projects\NationalGrid\nationalgridretailweb\Source\US Retail Web UI\USRetailWebApp\web.Test7.config"
			};

			foreach(string filename in filenamesToProcess)
			{
				factory.Process(filename);
			}

		}
	}
}
