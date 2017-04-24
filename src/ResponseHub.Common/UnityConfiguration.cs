using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;
using System.Diagnostics;

namespace Enivate.ResponseHub.Common
{
    public class UnityConfiguration
	{

		private const string _serviceNameKey = "ServiceName";

		private static IUnityContainer _container;
		public static IUnityContainer Container
		{
			get
			{
				if (_container == null)
				{
					throw new NullReferenceException("The UnityConfiguration Container property must be initialised before it is referenced.");
				}
				return _container;
			}
			set
			{
				_container = value;
			}
		}

		/// <summary>
		/// Writes the unity resolution issues to the event log.
		/// </summary>
		/// <param name="ex"></param>
		public static void LogUnityException(Exception ex)
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
