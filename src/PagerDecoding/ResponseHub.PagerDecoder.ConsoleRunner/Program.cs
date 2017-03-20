using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers;
using Enivate.ResponseHub.DataAccess.MongoDB;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.DataAccess.Interface;
using System.IO;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.ApplicationServices;

namespace Enivate.ResponseHub.PagerDecoder.ConsoleRunner
{
	class Program
	{
		
		static void Main(string[] args)
		{

			if (args.Length > 1 && args[0].ToLower() == "-pdw")
			{ 

				ILogger log = new FileLogger();
				IMapIndexRepository mapIndexRepo = new MapIndexRepository(log);
				IDecoderStatusRepository decoderStatusRepository = new DecoderStatusRepository();
				IJobMessageRepository jobMessageRepository = new JobMessageRepository();
				IJobMessageService jobMessageService = new JobMessageService(jobMessageRepository, log);

				// Create the PdwLogFileParser
				PdwLogFileParser pdwParser = new PdwLogFileParser(log, mapIndexRepo, decoderStatusRepository, jobMessageService);
				pdwParser.ProcessLogFiles();

				Console.WriteLine("Press any key to exit.");
				Console.Read();

			}
			else if (args.Length > 0 && args[0].ToLower() == "-addr")
			{

				// Get the path to the test messages
				string testMessagesFilePath = String.Format("{0}\\test_job_messages.txt", Environment.CurrentDirectory);

				// Ensure it exists
				if (!File.Exists(testMessagesFilePath))
				{
					Console.WriteLine(String.Format("Test message file path '{0}' not found.", testMessagesFilePath));
				}

				// Read the test messsages from the file
				IList<string> testMessages = new List<string>();
				using (StreamReader reader = new StreamReader(testMessagesFilePath))
				{
					while (reader.Peek() != -1)
					{
						testMessages.Add(reader.ReadLine());
					}
				}

				// Create our list to store the addresses in
				IList<string> addresses = new List<string>();

				// Create the address parser
				AddressParser parser = new AddressParser();

				// Loop through each message and extract the strings
				foreach(string message in testMessages)
				{
					// Get the address
					string address = parser.GetAddressFromString(message);

					// Write to the console and add to the list
					Console.WriteLine(address);
					addresses.Add(address);

				}

				// Now write all the addresses processed into a file
				string outputFilePath = String.Format("{0}\\address_output.txt", Environment.CurrentDirectory);
				using (StreamWriter writer = new StreamWriter(outputFilePath, false))
				{
					for (int i = 0; i < addresses.Count; i++)
					{
						writer.WriteLine(String.Format("{0}    ||    {1}", addresses[i], testMessages[i]));
					}
				}

				Console.WriteLine("Press any key to exit.");
				Console.Read();

			}

		}
	}
}
