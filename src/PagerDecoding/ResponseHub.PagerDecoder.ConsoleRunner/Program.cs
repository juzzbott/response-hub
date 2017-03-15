using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess;
using Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers;
using Enivate.ResponseHub.DataAccess.MongoDB;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.PagerDecoder.ApplicationServices;

namespace Enivate.ResponseHub.PagerDecoder.ConsoleRunner
{
	class Program
	{
		
		static void Main(string[] args)
		{

			ILogger log = new FileLogger();
			IMapIndexRepository mapIndexRepo = new MapIndexRepository(log);

			// Create the PdwLogFileParser
			PdwLogFileParser pdwParser = new PdwLogFileParser(log, mapIndexRepo);
			pdwParser.ProcessLogFiles();

			Console.WriteLine("Press any key to exit.");
			Console.Read();

		}
	}
}
