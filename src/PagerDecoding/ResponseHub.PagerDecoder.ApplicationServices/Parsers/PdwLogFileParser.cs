using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Addresses.Interface;

namespace Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers
{
	public class PdwLogFileParser : BasePagerSourceParser
	{

		/// <summary>
		/// The number of log file read attempts.
		/// </summary>
		private int _logFileAttempts = 0;

		/// <summary>
		/// The maximum log file attemts to check for using the previous day.
		/// </summary>
		private int _maxLogFileAttempts = 5;

		public PdwLogFileParser(ILogger log, IMapIndexRepository mapIndexRepository, IDecoderStatusRepository decoderStatusRepository, IJobMessageService jobMessageService, IAddressService addressService)
		{

			// Instantiate the interfaces.
			Log = log;
			MapIndexRepository = mapIndexRepository;
			DecoderStatusRepository = decoderStatusRepository;
			JobMessageService = jobMessageService;
			AddressService = addressService;

			// Initialise the list of pager messages to submit.
			PagerMessagesToSubmit = new List<PagerMessage>();
			JobMessagesToSubmit = new Dictionary<string, JobMessage>();

			// Instantiate the message parsers
			PagerMessageParser = new PagerMessageParser(Log);
			JobMessageParser = new JobMessageParser(AddressService, MapIndexRepository, Log);

		}

		public override void GetLatestMessages()
		{
			// Clear the last message list, just in case
			PagerMessagesToSubmit.Clear();
			JobMessagesToSubmit.Clear();

			// Get the last message sha
			LastInsertedMessageSha = GetLastMessageSha();

			// Process the messages in the log file
			ProcessPagerMessagesInLogFile(DateTime.Now);

			// Parse the pager messages to job messages and write to the database
			ParseAndSubmitJobMessages();

			// Write some stats to the log files.
			Log.Info(String.Format("Processed and submitted '{0}' job message{1}", JobMessagesToSubmit.Count, (JobMessagesToSubmit.Count != 1 ? "s" : "")));

			// Clear the lists
			PagerMessagesToSubmit.Clear();
			JobMessagesToSubmit.Clear();

		}

		/// <summary>
		/// Process the pager messages in the log file for the specific date. If the entire file is read without finding the last message sha, it will recurse to the previous days log file (if exists).
		/// </summary>
		/// <param name="logDate"></param>
		private void ProcessPagerMessagesInLogFile(DateTime logDate)
		{
			
			// Get the current log file filename
			string logFilePath = GetLogFilePath(logDate);

			// If the log file is empty, or the file doesn't exist, then log the issue and return
			if (String.IsNullOrEmpty(logFilePath) || !File.Exists(logFilePath))
			{

				// If we are within our max log file attempts, then recurse to previous day
				if (_logFileAttempts < _maxLogFileAttempts)
				{
					_logFileAttempts++;
					ProcessPagerMessagesInLogFile(logDate.AddDays(-1));
				}
				else if (!String.IsNullOrEmpty(logFilePath) && String.IsNullOrEmpty(LastInsertedMessageSha) && _logFileAttempts >= _maxLogFileAttempts)
				{
					// We have a log file name, but have recursed to a time when no date file exists, and we have no last message sha so it's the first run. 
					// Just log an info note
					Log.Info(String.Format("The log file '{0}' does not exist and no last message sha found in 5 day history. Assuming start of log file history. Exiting.", logFilePath));
					return;
				}
				else
				{
					// No valid log file found, when there should be one
					Log.Error(String.Format("The log file '{0}' does not exist.", logFilePath));
					return;
				}
			}
			else
			{

				// Get the messages from the log file.
				IList<string> rawMessages = GetMessagesFromLogFile(logFilePath);

				// Determines if the last message was reached or not. If false, after the loop, it will recurse into the previous days log files.
				bool lastMessageReached = false;

				// Loop through the messages
				foreach (string message in rawMessages)
                { 

					// If we should skip the message, then do so here.
                    if (ShouldSkipMessage(message))
                    {
						Log.Debug(String.Format("Skipping internal system message: {0}", message));
						continue;
                    }

					try
					{

						// Get the pager message
						PagerMessage pagerMessage = PagerMessageParser.ParsePagerMessage(message);
						
						// If the pager message is null, parsing failed, so continue to next
						if (pagerMessage == null)
						{
							continue;
						}

						// If the pager message is numeric, skip it
						if (pagerMessage.Type.ToUpper() != "ALPHA")
						{
							Log.Debug("Skipping non-alpha message.");
							continue;
						}

						// If the message appears invalid, flag it to be checked...
						if (MessageAppearsInvalid(pagerMessage.MessageContent))
						{
							Log.Warn(String.Format("Invalid message detected. Invalid message: {0}", pagerMessage.MessageContent));
							Task t = Task.Run(async () => {

								// Check to see if the message already exists
								bool messageExists = await DecoderStatusRepository.InvalidMessageExists(pagerMessage.MessageContent);

								// If it doesn't exist, add it
								if (!messageExists)
								{
									await DecoderStatusRepository.AddInvalidMessage(DateTime.UtcNow, message);
								}
							});
							t.Wait();
						}

						// If the pager sha matches the last inserted sha, then exit the loop, as no need to process any further messages.
						if (pagerMessage.ShaHash.Equals(LastInsertedMessageSha, StringComparison.CurrentCultureIgnoreCase))
						{
							lastMessageReached = true;
							Log.Info(String.Format("Last message hash detected. Exiting log file. Last message hash: '{0}'.", pagerMessage.ShaHash));
							break;
						}

						// Add the pager message to submit
						PagerMessagesToSubmit.Add(pagerMessage);

					}
					catch (Exception ex)
					{
						Log.Error(String.Format("Unable to parse pager message. Raw message data: {0}", message), ex);
					}

				}
				
				// If the last message wasn't reached, then recurse into the previous days log files
				if (!lastMessageReached)
				{
					Log.Info("End of log file detected. Processing previous log file.");
					ProcessPagerMessagesInLogFile(logDate.AddDays(-1));
				}

			}
		}

		/// <summary>
		/// Gets the list of pager messages from the log file. The messages are reversed in order, so that the most recent log entries are at the top of the list, so to minimise iterations.
		/// </summary>
		/// <param name="logFilePath">The path the to the log file to get messages from.</param>
		/// <returns>The list of raw pager messages that need to be parsed.</returns>
		private IList<string> GetMessagesFromLogFile(string logFilePath)
		{
			// Create the list of messages
			List<string> messages = new List<string>();

			// Begin using the stream reader
			// Ensure that we don't lock the file if the PDW application tries to write to the file.
			FileStream fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using (StreamReader reader = new StreamReader(fs))
			{
				while (reader.Peek() != -1)
				{
					// Read the line from the log file
					messages.Add(reader.ReadLine());
				}
			}

			// Reverse so that newest are at the top
			messages.Reverse();

			// return the messages
			return messages;
		}

		/// <summary>
		/// Gets the log file based on the specified date.
		/// </summary>
		/// <param name="date">The date of the log file.</param>
		/// <returns>The path to the log file.</returns>
		private string GetLogFilePath(DateTime date)
		{
			string basePath = ConfigurationManager.AppSettings["PDWLogDirectory"];

			return String.Format("{0}\\{1}.log", basePath, date.ToString("yyMMdd"));

		}

	}
}
