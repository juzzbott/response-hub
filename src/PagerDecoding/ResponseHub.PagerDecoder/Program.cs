using System;
using System.Collections.Generic;
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
		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{

			// Unity configuration loader
			UnityConfiguration.Container = new UnityContainer().LoadConfiguration();

			// Initialise the services
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new MessageService()
			};
			
			// Run windows service in normal service mode
			ServiceBase.Run(ServicesToRun);
						
		}
	}
}
