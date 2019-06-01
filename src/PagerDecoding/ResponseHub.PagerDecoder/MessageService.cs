using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using Microsoft.Practices.Unity;
using Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Logging.Configuration;
using Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers;
using Enivate.ResponseHub.Model.Addresses.Interface;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.PagerDecoder.ApplicationServices;

using Newtonsoft.Json;
using Enivate.ResponseHub.Model.Units.Interface;

namespace Enivate.ResponseHub.PagerDecoder
{
	public partial class MessageService : ServiceBase
	{

		/// <summary>
		/// The timer for checking for messages
		/// </summary>
		private Timer _msgServiceTimer;

		/// <summary>
		/// The timer for cleanup operations
		/// </summary>
		private Timer _cleanupTimer;

		/// <summary>
		/// The timer for invalid message checking
		/// </summary>
		private Timer _invalidMessageTimer;

		/// <summary>
		/// The timer interval key.
		/// </summary>
		private string _serviceIntervalKey = "ServiceTimerInterval";

        /// <summary>
        /// The timer interval key.
        /// </summary>
        private string _cleanupIntervalKey = "CleanupTimerInterval";

		/// <summary>
		/// The timer interval key.
		/// </summary>
		private string _invalidMessagesIntervalKey = "InvalidMessagesTimerInterval";

		/// <summary>
		/// The configuration key for the last message file.
		/// </summary>
		private string _lastMessageFileKey = "LastMessageFile";

		/// <summary>
		/// The configuration key for the last message file.
		/// </summary>
		private string _pagerMessageSourceKey = "PagerMessageSource";

		/// <summary>
		/// The source for the pager messages loaded from the configuration
		/// </summary>
		private string _pagerMessageSource;

		protected ILogger Log = ServiceLocator.Get<ILogger>();
		protected IMapIndexRepository MapIndexRepository = ServiceLocator.Get<IMapIndexRepository>();
		protected IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();
		protected IDecoderStatusRepository DecoderStatusRepository = ServiceLocator.Get<IDecoderStatusRepository>();
		protected IAddressService AddressService = ServiceLocator.Get<IAddressService>();
        protected ICapcodeService CapcodeService = ServiceLocator.Get<ICapcodeService>();

        public MessageService()
		{
			InitializeComponent();


			int timerInterval = 10000;
            int cleanupInterval = 86400000;
			int invalidMessagesInterval = 3600000;

			// Set pager message source
			_pagerMessageSource = ConfigurationManager.AppSettings[_pagerMessageSourceKey];

			// Validate the value, default to web as fallback
			if (String.IsNullOrEmpty(_pagerMessageSource))
			{
				Log.Debug(String.Format("{0} configuration value not set, defaulting to web", _pagerMessageSourceKey));
				_pagerMessageSource = "web";
			}

			// If there is no interval setting, then log warning
			if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[_serviceIntervalKey]))
			{
				Log.Warn("The configuration setting 'ServiceTimerInterval' is not present in the configuration. Defaulting to 10 seconds.");
			}
			else
			{
				// Get the timer interval
				Int32.TryParse(ConfigurationManager.AppSettings[_serviceIntervalKey], out timerInterval);
            }

            // If there is no cleanup interval setting, then log warning
            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[_cleanupIntervalKey]))
            {
                Log.Warn("The configuration setting 'CleanupTimerInterval' is not present in the configuration. Defaulting to 24 hours.");
            }
            else
            {
                // Get the timer interval
                Int32.TryParse(ConfigurationManager.AppSettings[_cleanupIntervalKey], out cleanupInterval);
            }
			
			// If there is no invalid messages interval setting, then log warning
			if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[_invalidMessagesIntervalKey]))
			{
				Log.Warn("The configuration setting 'InvalidMessagesTimerInterval' is not present in the configuration. Defaulting to 1 hour.");
			}
			else
			{
				// Get the timer interval
				Int32.TryParse(ConfigurationManager.AppSettings[_invalidMessagesIntervalKey], out invalidMessagesInterval);
			}

			// Initialise the message timer
			_msgServiceTimer = new Timer(timerInterval);
			_msgServiceTimer.AutoReset = false; // False so that the initial time run will pick up any pre-existing messages yet to be processed.
			_msgServiceTimer.Elapsed += _msgServiceTimer_Elapsed;

            // Initialise the cleanup timer
            _cleanupTimer = new Timer(cleanupInterval);
            _cleanupTimer.AutoReset = true;
            _cleanupTimer.Elapsed += _cleanupTimer_Elapsed;

			// Initialise the cleanup timer
			_invalidMessageTimer = new Timer(invalidMessagesInterval);
			_invalidMessageTimer.AutoReset = true;
			_invalidMessageTimer.Elapsed += _invalidMessageTimer_Elapsed;

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
            _cleanupTimer.Start();
			_invalidMessageTimer.Start();
		}

		/// <summary>
		/// Logic to Stop Service
		/// Public accessibility for running as a console application in Visual Studio debugging experience
		/// </summary>
		public virtual void StopService()
		{
			_msgServiceTimer.Stop();
            _cleanupTimer.Stop();
			_invalidMessageTimer.Stop();
			Log.Info("ResponseHub pager decoder service stopped.\r\n\r\n");
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
			sbStartLog.AppendLine("  ResponseHub pager decoder service started.");
			sbStartLog.AppendLine("==================================================");
			sbStartLog.AppendLine(String.Format("  Message Timer Interval: {0}", ConfigurationManager.AppSettings[_serviceIntervalKey]));
            sbStartLog.AppendLine(String.Format("  Cleanup Timer Interval: {0}", ConfigurationManager.AppSettings[_cleanupIntervalKey]));
			sbStartLog.AppendLine(String.Format("  Invalid Message Timer Interval: {0}", ConfigurationManager.AppSettings[_invalidMessagesIntervalKey]));
			sbStartLog.AppendLine(String.Format("  Pager Message Source: {0}", _pagerMessageSource));
			sbStartLog.AppendLine(String.Format("  Log Level: {0}", LoggingConfiguration.Current.LogLevel));
			sbStartLog.AppendLine(String.Format("  Log Directory: {0}", LoggingConfiguration.Current.LogDirectory));
			sbStartLog.AppendLine(String.Format("  Last Message File: {0}", ConfigurationManager.AppSettings[_lastMessageFileKey]));
			sbStartLog.AppendLine();
			return sbStartLog.ToString();
		}

		private void _msgServiceTimer_Elapsed(object sender, ElapsedEventArgs e)
		{

			Log.Debug("Message Timer elapsed.");

			try {

				// Create the PdwLogFileParser
				if (_pagerMessageSource.ToLower() == "pdw")
				{
					PdwLogFileParser pdwParser = new PdwLogFileParser(Log, MapIndexRepository, DecoderStatusRepository, JobMessageService, AddressService, CapcodeService);
					pdwParser.GetLatestMessages();
				}
				else
				{
					MazzanetWebParser webParser = new MazzanetWebParser(Log, MapIndexRepository, DecoderStatusRepository, JobMessageService, AddressService, CapcodeService);
					webParser.GetLatestMessages();
				}

			}
			catch (Exception ex)
			{
				Log.Error(String.Format("Error processing log file on timer elapse. Message: {0}", ex.Message), ex);
			}
			finally
			{


				// On the first start of the service, we want to process any log files yet to be processed. We do this prior to the timer being set as auto-reset, 
				// so that we don't get into a race condition with the response from a potentially large request.
				// However, after the first run, we want the time to auto reset, so if autoreset is false, set it to true and start the timer
				if (!_msgServiceTimer.AutoReset)
				{
					_msgServiceTimer.AutoReset = true;
					_msgServiceTimer.Start();
				}
			}

		}

        private void _cleanupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Log.Debug("Cleanup Timer elapsed.");

            try
            {

                // Perform the cleanup routine.

            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error performing cleanup. Message: {0}", ex.Message, ex));
            }
		}

		private async void _invalidMessageTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			await Log.Debug("Invalid Message Timer elapsed.");

			try
			{

				// Check invalid messages
				DecoderStatusService decoderService = new DecoderStatusService();
				await decoderService.CheckInvalidMessages();

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Error performing invalid message checking. Message: {0}", ex.Message, ex));
			}
		}

	}
}
