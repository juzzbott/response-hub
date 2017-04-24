using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.WeatherData.Interface;
using Enivate.ResponseHub.WebTasks.ApplicationServices;

using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;
using System.Diagnostics;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Model.Events.Interface;

namespace Enivate.ResponseHub.WebTasks.ConsoleRunner
{
	class Program
	{

		static void Main(string[] args)
		{

			try
			{
				// Unity configuration loader
				UnityConfiguration.Container = new UnityContainer().LoadConfiguration();
			}
			catch (Exception ex)
			{
				UnityConfiguration.LogUnityException(ex);
				return;
			}
			
			// Load the classes from unity
			ILogger log = ServiceLocator.Get<ILogger>();

			// If there is no arguments, show the warning
			if (args.Length == 0)
			{
				Console.WriteLine("You must specify options to perform the tasks:");
				Console.WriteLine("\t-bom - Run the caching of BoM files.");
				return;
			}

			if (args.Contains("-bom"))
			{

				// Get the weather data service
				IWeatherDataService weatherDataService = ServiceLocator.Get<IWeatherDataService>();

				// Get the bom cache file manager
				BomCacheManager cacheManager = new BomCacheManager(weatherDataService, log);

				log.Info("Executing task: Download BoM cache files.");

				// Remove any files that are older than the configured expiry time
				cacheManager.DeleteExpiredBomFiles();

				// Download the files required
				cacheManager.DownloadBomFilesForLocations();
			}
			else if (args.Contains("-eventjobs"))
			{
				
				IJobMessageService jobMessageService = ServiceLocator.Get<IJobMessageService>();
				IEventService eventService = ServiceLocator.Get<IEventService>();
				IUnitService unitService = ServiceLocator.Get<IUnitService>();
				ICapcodeService capcodeService = ServiceLocator.Get<ICapcodeService>();

				// Perform the routine to set the jobs for active events.
				EventJobMessageLoader eventJobLoader = new EventJobMessageLoader(eventService, jobMessageService, unitService, capcodeService, log);
				eventJobLoader.SetJobMessagesForActiveEvents();
			}

			Console.ReadLine();
		}
	}
}
