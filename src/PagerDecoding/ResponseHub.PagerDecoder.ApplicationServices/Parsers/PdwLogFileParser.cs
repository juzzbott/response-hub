using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.DataAccess.Interface;

namespace Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers
{
	public class PdwLogFileParser
	{

		/// <summary>
		/// The configuration key for the last message file.
		/// </summary>
		private string _lastMessageFileKey = "LastMessageFile";

		/// <summary>
		/// The configuration key for the web service url.
		/// </summary>
		private string _webServiceUrlKey = "ResponseHubService.Url";

		/// <summary>
		/// The configuration key for the web service api key.
		/// </summary>
		private string _webServiceUrlApiKeyKey = "ResponseHubService.ApiKey";

		/// <summary>
		/// The last inserted message hash.
		/// </summary>
		private string _lastInsertedMessageSha = "";

		/// <summary>
		/// Contains the list of pager messages to submit.
		/// </summary>
		public List<PagerMessage> PagerMessagesToSubmit { get; set; }

		/// <summary>
		/// Contains the list of parsed job messages to submit.
		/// </summary>
		public Dictionary<string, JobMessage> JobMessagesToSubmit { get; set; }

		/// <summary>
		/// The PagerMessage parser.
		/// </summary>
		private PagerMessageParser _pagerMessageParser;

		/// <summary>
		/// The JobMessage pager.
		/// </summary>
		private JobMessageParser _jobMessageParser;

		/// <summary>
		/// The log writer.
		/// </summary>
		private ILogger _log;

		/// <summary>
		/// The repository for managing map indexes.
		/// </summary>
		private IMapIndexRepository _mapIndexRepository;

		/// <summary>
		/// The number of log file read attempts.
		/// </summary>
		private int _logFileAttempts = 0;

		/// <summary>
		/// The maximum log file attemts to check for using the previous day.
		/// </summary>
		private int _maxLogFileAttempts = 5;

		public PdwLogFileParser(ILogger log, IMapIndexRepository mapIndexRepository)
		{

			// Instantiate the interfaces.
			_log = log;
			_mapIndexRepository = mapIndexRepository;

			// Initialise the list of pager messages to submit.
			PagerMessagesToSubmit = new List<PagerMessage>();
			JobMessagesToSubmit = new Dictionary<string, JobMessage>();

			// Instantiate the message parsers
			_pagerMessageParser = new PagerMessageParser(_log);
			_jobMessageParser = new JobMessageParser(_mapIndexRepository, _log);

		}

		public void ProcessLogFiles()
		{
			// Clear the last message list, just in case
			PagerMessagesToSubmit.Clear();
			JobMessagesToSubmit.Clear();

			// Get the last message sha
			_lastInsertedMessageSha = GetLastMessageSha();

			// Process the messages in the log file
			ProcessPagerMessagesInLogFile(DateTime.Now);
			
			// Parse the pager messages into job messages
			ParsePagerMessagesToJobMessages();

			// Submit the messages
			bool result = PostJobMessagesToWebService();

			if (result && PagerMessagesToSubmit.Count > 0)
			{
				// Get the last message sha
				_lastInsertedMessageSha = PagerMessagesToSubmit.Last().ShaHash;

				// Write the last message sha to the web service
				WriteLastMessageSha(_lastInsertedMessageSha);
			}

			// Write some stats to the log files.
			_log.Info(String.Format("Processed and submitted '{0}' job message{1}", JobMessagesToSubmit.Count, (JobMessagesToSubmit.Count != 1 ? "s" : "")));

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
				else if (!String.IsNullOrEmpty(logFilePath) && String.IsNullOrEmpty(_lastInsertedMessageSha) && _logFileAttempts >= _maxLogFileAttempts)
				{
					// We have a log file name, but have recursed to a time when no date file exists, and we have no last message sha so it's the first run. 
					// Just log an info note
					_log.Info(String.Format("The log file '{0}' does not exist and no last message sha found in 5 day history. Assuming start of log file history. Exiting.", logFilePath));
					return;
				}
				else
				{
					// No valid log file found, when there should be one
					_log.Error(String.Format("The log file '{0}' does not exist.", logFilePath));
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
					// If the message is null or empty, continue
					if (String.IsNullOrEmpty(message))
					{
						continue;
					}

					// Skip non-required messages.
					if (message.ToUpper().Contains(")E51TIMEUPDATE"))
					{
						_log.Debug("Skipping E51TIMEUPDATE time update message.");
						continue;
					}

					try
					{

						// Get the pager message
						PagerMessage pagerMessage = _pagerMessageParser.ParsePagerMessage(message);
						
						// If the pager message is null, parsing failed, so continue to next
						if (pagerMessage == null)
						{
							continue;
						}

						// If the pager message is numeric, skip it
						if (pagerMessage.Type.ToUpper() != "ALPHA")
						{
							_log.Debug("Skipping non-alpha message.");
							continue;
						}

						// If the pager sha matches the last inserted sha, then exit the loop, as no need to process any further messages.
						if (pagerMessage.ShaHash.Equals(_lastInsertedMessageSha, StringComparison.CurrentCultureIgnoreCase))
						{
							lastMessageReached = true;
							break;
						}

						// Add the pager message to submit
						PagerMessagesToSubmit.Add(pagerMessage);

					}
					catch (Exception ex)
					{
						_log.Error(String.Format("Unable to parse pager message. Raw message data: {0}", message), ex);
					}

				}
				
				// If the last message wasn't reached, then recurse into the previous days log files
				if (!lastMessageReached)
				{
					_log.Info("End of log file detected. Processing previous log file.");
					ProcessPagerMessagesInLogFile(logDate.AddDays(-1));
				}

			}
		}

		#region Submit messages
		
		/// <summary>
		/// Posts the messages to the webservice
		/// </summary>
		/// <returns></returns>
		private bool PostJobMessagesToWebService()
		{
			// Get the json string for the list of pager messages
			string jsonData = JsonConvert.SerializeObject(JobMessagesToSubmit.Select(i => i.Value).ToArray());
			byte[] jsonBytes = Encoding.ASCII.GetBytes(jsonData);

			// Get the service url and api key
			string serviceUrl = ConfigurationManager.AppSettings[_webServiceUrlKey];
			string serviceApiKey = ConfigurationManager.AppSettings[_webServiceUrlApiKeyKey];

			// If the service url is null or empty, throw exception
			if (String.IsNullOrEmpty(serviceUrl))
			{
				throw new ApplicationException("The web service url configuration is missing or empty.");
			}

			// Create the post request
			HttpWebRequest request = WebRequest.CreateHttp(serviceUrl);
			request.Method = "POST";
			request.ContentType = "application/json";
			request.ContentLength = jsonBytes.Length;
			request.Headers.Add(HttpRequestHeader.Authorization, String.Format("APIKEY {0}", serviceApiKey));
			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(jsonBytes, 0, jsonBytes.Length);
			}

			// Create the message sha variable
			string responseText = "";

			// Get the response
			//Task.Run(async () =>
			//{
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				// If all went well, set the last message sha to the last in the list of jbo messages
			if (response.StatusCode == HttpStatusCode.OK)
			{
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{
					responseText = reader.ReadToEnd();
				}
			}

			//}).Wait();

			// return the message sha
			return (responseText.ToLower() == "true");

		}

		/// <summary>
		/// Parse the pager messages into JobMessages.
		/// </summary>
		private void ParsePagerMessagesToJobMessages()
		{
			// Loop through the pager messages
			foreach (PagerMessage pagerMessage in PagerMessagesToSubmit)
			{

				try
				{

					// Get the job message from the pager message
					JobMessage jobMessage = _jobMessageParser.ParseMessage(pagerMessage);

					// Add the job to the list job messages to submit
					JobMessagesToSubmit.Add(pagerMessage.ShaHash, jobMessage);

				}
				catch (Exception ex)
				{
					// Log the error the log file
					_log.Error(String.Format("Unable to parse pager message. Message: {0}", ex.Message), ex);
					_log.Error(String.Format("Pager message that failed: {0}", pagerMessage.ToString()));
				}

			}
		}

		#endregion

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


		#region Last Message Inserted Helpers

		/// <summary>
		/// Gets the path to the file of the last message inserted. 
		/// </summary>
		/// <returns>The last message inserted file path.</returns>
		private string GetLastMessageFilePath()
		{
			// Get the path from the confirguration
			string path = ConfigurationManager.AppSettings[_lastMessageFileKey];

			// If the path is null or empty, throw exception
			if (String.IsNullOrEmpty(path))
			{
				throw new ApplicationException(String.Format("The configuration key '{0}' is missing or is empty.", _lastMessageFileKey));
			}

			// If it begins with a '.', map to curent directory without first .
			if (path[0] == '.')
			{
				path = String.Format("{0}{1}", Environment.CurrentDirectory, path.Substring(1));
			}

			// return the path
			return path;
		}

		/// <summary>
		/// Gets the SHA hash of the the last message inserted.
		/// </summary>
		/// <returns>The sha of the last message inserted.</returns>
		private string GetLastMessageSha()
		{
			// Create the storage var
			string lastMessageSha = "";

			// Get the file path
			string lastMessagePath = GetLastMessageFilePath();

			if (File.Exists(lastMessagePath))
			{
				// Read the contents of the last message file.
				using (StreamReader reader = new StreamReader(lastMessagePath))
				{
					lastMessageSha = reader.ReadToEnd();
				}
			} 

			// return the last message sha
			return lastMessageSha;
		}

		/// <summary>
		/// Writes the last message sha to the file.
		/// </summary>
		/// <param name="lastMessageSha">The sha has of the last message to write.</param>
		private void WriteLastMessageSha(string lastMessageSha)
		{
			// If the last message sha is null, empty or whitespace, log and return without writing anyting
			if (String.IsNullOrWhiteSpace(lastMessageSha))
			{
				_log.Warn("The last message sha could not be written as the sha value was empty.");
				return;
			}

			// Write the sha to the file
			using (StreamWriter writer = new StreamWriter(GetLastMessageFilePath(), false))
			{
				writer.Write(lastMessageSha);
			}

		}

		#endregion

	}
}
