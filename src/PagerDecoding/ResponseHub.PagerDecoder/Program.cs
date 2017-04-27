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
				// Write the exception
				UnityConfiguration.LogUnityException(ex);
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
	}
}
