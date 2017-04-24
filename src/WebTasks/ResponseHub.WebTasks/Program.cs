using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Common;
using Microsoft.Practices.Unity;

using Microsoft.Practices.Unity.Configuration;

namespace Enivate.ResponseHub.WebTasks
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
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

			// Initialise the services
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new WebTasksService()
			};

			// Run windows service in normal service mode
			ServiceBase.Run(ServicesToRun);
		}
	}
}
