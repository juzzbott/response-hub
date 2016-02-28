using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using DotSpatial.Data;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Logging.Configuration;
using Enivate.ResponseHub.Model.Spatial;
using DotSpatial.Projections;
using Enivate.ResponseHub.WindowsService.Parsers;
using Enivate.ResponseHub.Model.Messages;

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
		private string _intervalKey = "Timer.Interval";

		/// <summary>
		/// The last inserted message hash.z
		/// </summary>
		private string _lastInsertedMessageSha = "";

		private List<PagerMessage> _messagesToSubmit;

		private ILogger _log;
		protected ILogger Log
		{
			get
			{
				return _log ?? (_log = UnityConfiguration.Container.Resolve<ILogger>());
			}
		}

		private PagerMessageParser _pagerMessageParser;

		public MessageService()
		{
			InitializeComponent();

			// Initialise the list of pager messages to submit.
			_messagesToSubmit = new List<PagerMessage>();

			// Instantiate the message parser
			_pagerMessageParser = new PagerMessageParser();

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

				// Get the current log file filename
				string logFilePath = GetLogFilePath(DateTime.Now);

				// If the log file is empty, or the file doesn't exist, then log the issue and return
				if (String.IsNullOrEmpty(logFilePath) || !File.Exists(logFilePath))
				{
					Log.Error(String.Format("The log file '{0}' does not exist.", logFilePath));
					return;
				}

				// Get the messages from the log file.
				IList<string> rawMessages = GetMessagesFromLogFile(logFilePath);

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
							break;
						}

						// Add the pager message to submit
						_messagesToSubmit.Add(pagerMessage);

					}
					catch (Exception ex)
					{
						Log.Error(String.Format("Unable to parse pager message. Raw message data: {0}", message), ex);
					}

				}

				// Submit the messages
				SubmitMessages();

			}
			catch (Exception ex)
			{
				Log.Error(String.Format("Error processing log file on timer elapse. Message: {0}", ex.Message), ex);
			}

		}

		private void SubmitMessages()
		{
			throw new NotImplementedException();
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
			string basePath = ConfigurationManager.AppSettings["PDW.LogDirectory"];

			return String.Format("{0}\\yyMMdd.log");

		}
	}
}
