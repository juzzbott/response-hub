using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

using Enivate.ResponseHub.Common;

namespace Enivate.ResponseHub.PagerDecoder
{
	static class Program
	{

		private const string _serviceNameKey = "ServiceName";
		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{

			try
			{
				// Unity configuration loader
				UnityConfiguration.Container = new UnityContainer().LoadConfiguration();
			}
			catch (Exception ex)
			{
				LogUnityException(ex);
				return;
			}

			// Initialise the services
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new MessageService()
			};
			
			// Run windows service in normal service mode
			ServiceBase.Run(ServicesToRun);
						
		}

		/// <summary>
		/// Writes the unity resolution issues to the event log.
		/// </summary>
		/// <param name="ex"></param>
		private static void LogUnityException(Exception ex)
		{
			string source = ConfigurationManager.AppSettings[_serviceNameKey];
			string log = "Application";
			string eventDesc = String.Format("Failure loading unity configurations. Message: {0}\r\n{1}", ex.Message, ex.StackTrace);

			// If the event source doesn't exist, create it
			if (!EventLog.SourceExists(source))
			{
				EventLog.CreateEventSource(source, log);
			}

			// Write the log entry
			EventLog.WriteEntry(source, eventDesc, EventLogEntryType.Error, 0001);


		}
	}
}
