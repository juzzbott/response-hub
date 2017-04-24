using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.WeatherData.Interface;
using Enivate.ResponseHub.Model.Messages.Interface;
using System.Configuration;
using Enivate.ResponseHub.Logging.Configuration;
using Enivate.ResponseHub.WebTasks.ApplicationServices;
using Enivate.ResponseHub.Model.Events.Interface;
using Enivate.ResponseHub.Model.Units.Interface;

namespace Enivate.ResponseHub.WebTasks
{
	public partial class WebTasksService : ServiceBase
	{

		/// <summary>
		/// The timer for updating bom cache files.
		/// </summary>
		private Timer _bomCacheServiceTimer;

		/// <summary>
		/// The timer for checking for new messages for events
		/// </summary>
		private Timer _eventMessageServiceTimer;

		/// <summary>
		/// The bom cache timer interval key.
		/// </summary>
		private string _bomCacheIntervalKey = "BomCacheTimerInterval";

		/// <summary>
		/// The event message timer interval key.
		/// </summary>
		private string _eventMessageIntervalKey = "EventMessageTimerInterval";

		protected ILogger Log = ServiceLocator.Get<ILogger>();
		protected IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();
		protected IEventService EventService = ServiceLocator.Get<IEventService>();
		protected IUnitService UnitService = ServiceLocator.Get<IUnitService>();
		protected ICapcodeService CapcodeService = ServiceLocator.Get<ICapcodeService>();
		protected IWeatherDataService WeatherDataService = ServiceLocator.Get<IWeatherDataService>();


		public WebTasksService()
		{
			InitializeComponent();

			// Set the default timer values
			int bomCacheInterval = 300000;
			int eventMessageInterval = 60000;

			// If there is no bom cache interval setting, then log warning
			if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[_bomCacheIntervalKey]))
			{
				Log.Warn("The configuration setting 'BomCacheTimerInterval' is not present in the configuration. Defaulting to 5 minutes.");
			}
			else
			{
				// Get the timer interval
				Int32.TryParse(ConfigurationManager.AppSettings[_bomCacheIntervalKey], out bomCacheInterval);
			}

			// If there is no event message interval setting, then log warning
			if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[_eventMessageIntervalKey]))
			{
				Log.Warn("The configuration setting 'EventMessageTimerInterval' is not present in the configuration. Defaulting to 60 seconds.");
			}
			else
			{
				// Get the timer interval
				Int32.TryParse(ConfigurationManager.AppSettings[_eventMessageIntervalKey], out eventMessageInterval);
			}

			// Initialise the bom cache timer
			_bomCacheServiceTimer = new Timer(bomCacheInterval);
			_bomCacheServiceTimer.AutoReset = true;
			_bomCacheServiceTimer.Elapsed += _bomCacheServiceTimer_Elapsed; ;

			// Initialise the event message timer
			_eventMessageServiceTimer = new Timer(eventMessageInterval);
			_eventMessageServiceTimer.AutoReset = true;
			_eventMessageServiceTimer.Elapsed += _eventMessageServiceTimer_Elapsed; ;
		}

		protected override void OnStart(string[] args)
		{
			// Log the start event.
			string startLog = buildStartupLog();
			Log.Info(startLog);

			_bomCacheServiceTimer.Start();
			_eventMessageServiceTimer.Start();
		}

		protected override void OnStop()
		{
			_bomCacheServiceTimer.Stop();
			_eventMessageServiceTimer.Stop();
			Log.Info("ResponseHub web tasks service stopped.\r\n\r\n");
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
			sbStartLog.AppendLine("  ResponseHub web tasks service started.");
			sbStartLog.AppendLine("==================================================");
			sbStartLog.AppendLine(String.Format("  BoM Cache Timer Interval: {0}", ConfigurationManager.AppSettings[_bomCacheIntervalKey]));
			sbStartLog.AppendLine(String.Format("  Event Manager Timer Interval: {0}", ConfigurationManager.AppSettings[_eventMessageIntervalKey]));
			sbStartLog.AppendLine(String.Format("  Log Level: {0}", LoggingConfiguration.Current.LogLevel));
			sbStartLog.AppendLine(String.Format("  Log Directory: {0}", LoggingConfiguration.Current.LogDirectory));
			sbStartLog.AppendLine();
			return sbStartLog.ToString();
		}

		private void _bomCacheServiceTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Log.Debug("BoM Cache Timer elapsed.");

			try
			{

				// Get the bom cache file manager
				BomCacheManager cacheManager = new BomCacheManager(WeatherDataService, Log);

				// Remove any files that are older than the configured expiry time
				cacheManager.DeleteExpiredBomFiles();

				// Download the files required
				cacheManager.DownloadBomFilesForLocations();

			}
			catch (Exception ex)
			{
				Log.Error(String.Format("Error refreshing BoM cache files. Message: {0}", ex.Message, ex));
			}
		}

		private void _eventMessageServiceTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Log.Debug("Event Message Timer elapsed.");

			try
			{

				// Perform the routine to set the jobs for active events.
				EventJobMessageLoader eventJobLoader = new EventJobMessageLoader(EventService, JobMessageService, UnitService, CapcodeService, Log);
				eventJobLoader.SetJobMessagesForActiveEvents();

			}
			catch (Exception ex)
			{
				Log.Error(String.Format("Error getting latest event messages. Message: {0}", ex.Message, ex));
			}
		}
	}
}
