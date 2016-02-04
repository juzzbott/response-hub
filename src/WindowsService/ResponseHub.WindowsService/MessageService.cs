using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Logging.Configuration;

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

		private ILogger _log;
		protected ILogger Log
		{
			get
			{
				return _log ?? (_log = UnityConfiguration.Container.Resolve<ILogger>());
			}
		}

		public MessageService()
		{
			InitializeComponent();

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

		}
	}
}
