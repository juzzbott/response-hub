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

			// By default, just run the PDW log parser
			if (args == null || args.Length == 0)
			{

				ILogger log = new FileLogger();
				IMapIndexRepository mapIndexRepo = new MapIndexRepository(log);

				// Create the PdwLogFileParser
				//PsdwLogFileParser pdwParser = new PdwLogFileParser(log, mapIndexRepo);
				//pdwParser.ProcessLogFiles();
			}
			else if (args[0].ToLower() == "-im")
			{

				DecoderStatusRepository repo = new DecoderStatusRepository();

				// Add some invalid messages
				for (int i = 0; i < 11; i++)
				{
					Task.Run(async () => await repo.AddInvalidMessage(DateTime.UtcNow.AddMinutes(i * -1), "*U*U*U*??)")).Wait();
				}

				DecoderStatusService decoderService = new DecoderStatusService();
				Task.Run(async () => await decoderService.CheckInvalidMessages()).Wait();

			}

			Console.WriteLine("Press any key to exit.");
			Console.Read();

		}
	}
}
