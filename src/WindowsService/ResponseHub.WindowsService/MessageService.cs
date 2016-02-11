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

			// Load the index maps into memory
			//LoadIndexMaps();

			// Initialise the timer
			_msgServiceTimer = new Timer(timerInterval);
			_msgServiceTimer.AutoReset = true;
			_msgServiceTimer.Elapsed += _msgServiceTimer_Elapsed;

		}

		/// <summary>
		/// Loads the index maps from disk into memory as a collection of searchable map references.
		/// </summary>
		private void LoadIndexMaps()
		{

			// Set the filename variable
			string filename = Environment.CurrentDirectory + "\\App_Data\\MapIndexFiles\\SpatialVision\\MAPBOOK_PAGEGRID_50_ED4.shp";
			string projectionFile = Environment.CurrentDirectory + "\\App_Data\\MapIndexFiles\\SpatialVision\\MAPBOOK_PAGEGRID_50_ED4.prj";

			try
			{

				DateTime startDate = DateTime.Now;

				// Load the shape file and project to GDA94
				Shapefile indexMapFile = Shapefile.OpenFile(filename);
				indexMapFile.Reproject(KnownCoordinateSystems.Geographic.Australia.GeocentricDatumofAustralia1994);				

				foreach(IFeature feature in indexMapFile.Features.ToList())
				{
				
					// Get the map values
					int pageNumber;
					Int32.TryParse(feature.DataRow[2].ToString(), out pageNumber);
					
					int mgaZone;
					Int32.TryParse(feature.DataRow[4].ToString(), out mgaZone);
					
					int scale;
					Int32.TryParse(feature.DataRow[5].ToString(), out scale);
				
					// iterate through the features of the map index, and create the MapReference object. 
					MapReference mapRef = new MapReference()
					{
						GridReference = feature.DataRow[3].ToString(),
						PageNumber = pageNumber,
						Scale = scale,
						UtmNumber = mgaZone
					};
				
					// Add map ref to cache.
					MapReferenceCache.Instance.AddMapReference(MapType.SpatialVision, mapRef);
				
				}

				DateTime endDate = DateTime.Now;
				TimeSpan diff = (endDate - startDate);

				using (StreamWriter writer = new StreamWriter(Environment.CurrentDirectory + "\\times.txt"))
				{
					writer.WriteLine("Start date: " + startDate.ToString("HH:mm:ss.fff"));
					writer.WriteLine("Start date: " + endDate.ToString("HH:mm:ss.fff"));
					writer.WriteLine("Difference milliseconds: " + diff.TotalMilliseconds.ToString());
				}

				// Close the index map file.
				indexMapFile.Close();

			}
			catch (Exception ex)
			{
				_log.Error(String.Format("Unable to load map index file: {0}", filename));

				// rethrow exeption to exit
				throw;
			}
			
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
