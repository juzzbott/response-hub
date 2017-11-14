using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.DataAccess.Interface;
using System.IO;
using Enivate.ResponseHub.Model.Messages.Interface;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Addresses.Interface;

using Microsoft.Practices.Unity.Configuration;
using Unity;

namespace Enivate.ResponseHub.PagerDecoder.ConsoleRunner
{
	class Program
	{

		protected ILogger Log
		{
			get
			{
				return ServiceLocator.Get<ILogger>();
			}
		}

		protected IMapIndexRepository MapIndexRepository
		{
			get
			{
				return ServiceLocator.Get<IMapIndexRepository>();
			}
		}

		protected IJobMessageService JobMessageService
		{
			get
			{
				return ServiceLocator.Get<IJobMessageService>();
			}
		}

		protected IDecoderStatusRepository DecoderStatusRepository
		{
			get
			{
				return ServiceLocator.Get<IDecoderStatusRepository>();
			}
		}

		protected IAddressService AddressSerice
		{
			get
			{
				return ServiceLocator.Get<IAddressService>();
			}
		}

		static void Main(string[] args)
		{

			// Unity configuration loader
			UnityConfiguration.Container = new UnityContainer().LoadConfiguration();

			ILogger log = ServiceLocator.Get<ILogger>();
			IMapIndexRepository mapIndexRepository = ServiceLocator.Get<IMapIndexRepository>();
			IDecoderStatusRepository decoderStatusRepository = ServiceLocator.Get<IDecoderStatusRepository>();
			IJobMessageService jobMessageService = ServiceLocator.Get<IJobMessageService>();
			IAddressService addressService = ServiceLocator.Get<IAddressService>();

			if (args.Length > 0 && args[0].ToLower() == "-pdw")
			{ 
				
				// Create the PdwLogFileParser
				PdwLogFileParser pdwParser = new PdwLogFileParser(log, mapIndexRepository, decoderStatusRepository, jobMessageService, addressService);
				pdwParser.GetLatestMessages();

				Console.WriteLine("Press any key to exit.");
				Console.Read();

			}

			if (args.Length > 0 && args[0].ToLower() == "-web")
			{

				// Create the PdwLogFileParser
				MazzanetWebParser pdwParser = new MazzanetWebParser(log, mapIndexRepository, decoderStatusRepository, jobMessageService, addressService);
				pdwParser.GetLatestMessages();

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
					string address = parser.GetAddressFromMessage(message);

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
