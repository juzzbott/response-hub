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
using Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers;
using Enivate.ResponseHub.Model.Messages;

using Newtonsoft.Json;
using System.Threading.Tasks;
using Enivate.ResponseHub.DataAccess.Interface;

namespace Enivate.ResponseHub.PagerDecoder
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

		/// <summary>
		/// The configuration key for the last message file.
		/// </summary>
		private string _lastMessageFileKey = "LastMessageFile";

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

		public MessageService()
		{
			InitializeComponent();


			int timerInterval = 10000;

			// If there is no interval setting, then throw exception
			if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[_intervalKey]))
			{
				Log.Error("The configuration setting 'Timer.Interval' is not present in the configuration. Defaulting to 10 seconds.");
			}
			else
			{
				// Get the timer interval
				Int32.TryParse(ConfigurationManager.AppSettings[_intervalKey], out timerInterval);
			}
			
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
			sbStartLog.AppendLine(String.Format("  Last Message File: {0}", ConfigurationManager.AppSettings[_lastMessageFileKey]));
			sbStartLog.AppendLine();
			return sbStartLog.ToString();
		}

		private void _msgServiceTimer_Elapsed(object sender, ElapsedEventArgs e)
		{

			Log.Debug("Timer elapsed.");

			try {

				// Create the PdwLogFileParser
				PdwLogFileParser pdwParser = new PdwLogFileParser(Log, MapIndexRepository);
				pdwParser.ProcessLogFiles();

			}
			catch (Exception ex)
			{
				Log.Error(String.Format("Error processing log file on timer elapse. Message: {0}", ex.Message), ex);
			}

		}

	}
}
