using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Timers;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Logging.Configuration;
using Enivate.ResponseHub.WindowsService.Parsers;
using Enivate.ResponseHub.Model.Messages;

using Newtonsoft.Json;
using System.Threading.Tasks;
using Enivate.ResponseHub.DataAccess.Interface;

namespace Enivate.ResponseHub.WindowsService
{
	public partial class MessageService : ServiceBase
	{

		/// <summary>
		/// The timer for checking for messages
		/// </summary>
		private Timer _msgServiceTimer;

		/// <summary>
		/// The timer interval key.
		/// </summary>
		private string _intervalKey = "ServiceTimerInterval";

		private string _lastMessageFileKey = "LastMessageFile";

		private string _webServiceUrlKey = "ResponseHubService.Url";

		/// <summary>
		/// The last inserted message hash.z
		/// </summary>
		private string _lastInsertedMessageSha = "";

		/// <summary>
		/// Contains the list of pager messages to submit.
		/// </summary>
		private List<PagerMessage> _pagerMessagesToSubmit;

		/// <summary>
		/// Contains the list of parsed job messages to submit.
		/// </summary>
		private Dictionary<string, JobMessage> _jobMessagesToSubmit;

		/// <summary>
		/// The PagerMessage parser.
		/// </summary>
		private PagerMessageParser _pagerMessageParser;

		/// <summary>
		/// The JobMessage pager.
		/// </summary>
		private JobMessageParser _jobMessageParser;

		private ILogger _log;
		protected ILogger Log
		{
			get
			{
				return _log ?? (_log = UnityConfiguration.Container.Resolve<ILogger>());
			}
		}

		private IMapIndexRepository _mapIndexRepository;
		protected IMapIndexRepository MapIndexRepository
		{
			get
			{
				return _mapIndexRepository ?? (_mapIndexRepository = UnityConfiguration.Container.Resolve<IMapIndexRepository>());
			}
		}

		public MessageService()
		{
			InitializeComponent();

			// Initialise the list of pager messages to submit.
			_pagerMessagesToSubmit = new List<PagerMessage>();
			_jobMessagesToSubmit = new Dictionary<string, JobMessage>();

			// Instantiate the message parsers
			_pagerMessageParser = new PagerMessageParser();
			_jobMessageParser = new JobMessageParser(MapIndexRepository);

			// If there is no interval setting, then throw exception
			if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[_intervalKey]))
			{
				throw new ApplicationException("The configuration setting 'Timer.Interval' is not present in the configuration.");
			}

			// Get the timer interval
			double timerInterval;
			double.TryParse(ConfigurationManager.AppSettings[_intervalKey], out timerInterval);
			
			// Initialise the timer
			_msgServiceTimer = new Timer(timerInterval);
			_msgServiceTimer.AutoReset = true;
			_msgServiceTimer.Elapsed += _msgServiceTimer_Elapsed;

		}
		
		protected override void OnStart(string[] args)
		{
			StartService(args);
		}

		protected override void OnStop()
		{
			StopService();
		}

		/// <summary>
		/// Logic to Start Service
		/// Public accessibility for running as a console application in Visual Studio debugging experience
		/// </summary>
		public virtual void StartService(params string[] args)
		{
			// Log the start event.
			string startLog = buildStartupLog();
			Log.Info(startLog);

			_msgServiceTimer.Start();
		}

		/// <summary>
		/// Logic to Stop Service
		/// Public accessibility for running as a console application in Visual Studio debugging experience
		/// </summary>
		public virtual void StopService()
		{
			_msgServiceTimer.Stop();
			Log.Info("ResponseHub service stopped.\r\n\r\n");
		}

		/// <summary>
		/// Builds the start up log for the windows service.
		/// </summary>
		/// <returns></returns>
		private string buildStartupLog()
		{
			// Build the start up log
			StringBuilder sbStartLog = new StringBuilder();
			sbStartLog.AppendLine();
			sbStartLog.AppendLine("==================================================");
			sbStartLog.AppendLine("  ResponseHub service started.");
			sbStartLog.AppendLine("==================================================");
			sbStartLog.AppendLine(String.Format("  Timer Interval: {0}", ConfigurationManager.AppSettings[_intervalKey]));
			sbStartLog.AppendLine(String.Format("  Log Level: {0}", LoggingConfiguration.Current.LogLevel));
			sbStartLog.AppendLine(String.Format("  Log Directory: {0}", LoggingConfiguration.Current.LogDirectory));
			return sbStartLog.ToString();
		}

		private void _msgServiceTimer_Elapsed(object sender, ElapsedEventArgs e)
		{

			Log.Debug("Timer elapsed.");

			try {

				// Clear the last message list, just in case
				_pagerMessagesToSubmit.Clear();
				_jobMessagesToSubmit.Clear();

				// Get the last message sha
				_lastInsertedMessageSha = GetLastMessageSha();

				// Process the messages in the log file
				ProcessPagerMessagesInLogFile(DateTime.Now);

				// Submit the messages
				SubmitMessages();

			}
			catch (Exception ex)
			{
				Log.Error(String.Format("Error processing log file on timer elapse. Message: {0}", ex.Message), ex);
			}

		}

		private void ProcessPagerMessagesInLogFile(DateTime logDate)
		{


			// Get the current log file filename
			string logFilePath = GetLogFilePath(logDate);

			// If the log file is empty, or the file doesn't exist, then log the issue and return
			if (String.IsNullOrEmpty(logFilePath) || !File.Exists(logFilePath))
			{
				Log.Error(String.Format("The log file '{0}' does not exist.", logFilePath));
				return;
			}

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

				try
				{

					// Get the pager message
					PagerMessage pagerMessage = _pagerMessageParser.ParsePagerMessage(message);

					// If the pager message is numeric, skip it
					if (pagerMessage.Type.ToUpper() != "ALPHA")
					{
						Log.Debug("Skipping non-alpha message.");
						continue;
					}

					// If the pager sha matches the last inserted sha, then exit the loop, as no need to process any further messages.
					if (pagerMessage.ShaHash.Equals(_lastInsertedMessageSha, StringComparison.CurrentCultureIgnoreCase))
					{
						lastMessageReached = true;
						break;
					}

					// Add the pager message to submit
					_pagerMessagesToSubmit.Add(pagerMessage);

				}
				catch (Exception ex)
				{
					Log.Error(String.Format("Unable to parse pager message. Raw message data: {0}", message), ex);
				}

				// If the last message wasn't reached, then recurse into the previous days log files
				if (!lastMessageReached)
				{
					ProcessPagerMessagesInLogFile(logDate.AddDays(-1));
				}

			}
		}

		#region Submit messages

		/// <summary>
		/// Saves the pager messages to the local database for historical reasons. Submits the parsed JobMessages to the webservice for consumption by the web application.
		/// </summary>
		private void SubmitMessages()
		{

			// Parse the pager messages into job messages
			ParsePagerMessagesToJobMessages();

			// Post the parsed job messages to the web api service
			string lastMessageSha = PostJobMessagesToWebService();

			// Write the last message sha to the web service
			WriteLastMessageSha(lastMessageSha);

			// Clear the lists
			_pagerMessagesToSubmit.Clear();
			_jobMessagesToSubmit.Clear();

			// Write some stats to the log files.
			Log.Debug(String.Format("Processed and submitted '{0}' job message{1}", _jobMessagesToSubmit.Count, (_jobMessagesToSubmit.Count != 1 ? "s" : "")));

		}

		/// <summary>
		/// Posts the messages to the webservice
		/// </summary>
		/// <returns></returns>
		private string PostJobMessagesToWebService()
		{
			// Get the json string for the list of pager messages
			string jsonData = JsonConvert.SerializeObject(_jobMessagesToSubmit);
			byte[] jsonBytes = Encoding.ASCII.GetBytes(jsonData);
			// Get the service url
			string serviceUrl = ConfigurationManager.AppSettings[_webServiceUrlKey];

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
			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(jsonBytes, 0, jsonBytes.Length);
			}

			// Create the message sha variable
			string messageSha = "";

			// Get the response
			Task.Run(async () =>
			{
				HttpWebResponse response = ((HttpWebResponse)await request.GetResponseAsync());
				
				// If all went well, set the last message sha to the last in the list of jbo messages
				if (response.StatusCode == HttpStatusCode.OK)
				{
					messageSha = _jobMessagesToSubmit.Last().Key;
				}

			});

			// return the message sha
			return messageSha;

		}

		/// <summary>
		/// Parse the pager messages into JobMessages.
		/// </summary>
		private void ParsePagerMessagesToJobMessages()
		{
			// Loop through the pager messages
			foreach (PagerMessage pagerMessage in _pagerMessagesToSubmit)
			{

				try
				{

					// Get the job message from the pager message
					JobMessage jobMessage = _jobMessageParser.ParseMessage(pagerMessage);

					// Add the job to the list job messages to submit
					_jobMessagesToSubmit.Add(pagerMessage.ShaHash, jobMessage);

				}
				catch (Exception ex)
				{
					// Log the error the log file
					Log.Error(String.Format("Unable to parse pager message. Message: {0}", ex.Message), ex);
					Log.Error(String.Format("Pager message that failed: {0}", pagerMessage.ToString()));
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
				messages.Add(reader.ReadLine());
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

			return String.Format("{0}\\yyMMdd.log");

		}

		#region Last Message Inserted

		/// <summary>
		/// Gets the path to the file of the last message inserted. 
		/// </summary>
		/// <returns>The last message inserted file path.</returns>
		private string GetLastMessageFile()
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

			using (StreamReader reader = new StreamReader(GetLastMessageFile()))
			{
				lastMessageSha = reader.ReadToEnd();
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
				Log.Warn("The last message sha could not be written as the sha value was empty.");
				return;
			}

			// Write the sha to the file
			using (StreamWriter writer = new StreamWriter(GetLastMessageFile(), false))
			{
				writer.Write(lastMessageSha);
			}

		}

		#endregion

	}
}
