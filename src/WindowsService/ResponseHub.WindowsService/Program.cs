using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

using Enivate.ResponseHub.Common;

namespace Enivate.ResponseHub.WindowsService
{
	static class Program
	{

		/// <summary>
		/// Determines if the service should be run in console mode.
		/// </summary>
		static bool _runInConsole = false;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{

			if (args.Contains("-console"))
			{
				_runInConsole = true;
			}

			// Unity configuration loader
			UnityConfiguration.Container = new UnityContainer().LoadConfiguration();

			// Initialise the services
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new MessageService()
				{
					ServiceName = "ResponseHub"
				}
			};
			
			if (_runInConsole)
			{

				// Start in command line mode for each of the services
				foreach(MessageService service in ServicesToRun)
				{
					service.StartService(args);
				}
				
			}
			else
			{
				// Run windows service in normal service mode
				ServiceBase.Run(ServicesToRun);
			}

			
		}
	}
}
