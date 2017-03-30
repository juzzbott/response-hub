using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;

namespace Enivate.ResponseHub.UI.Filters
{
	public class ExceptionLogFilter : IExceptionFilter
	{
		
		ILogger Log
		{
			get
			{
				return ServiceLocator.Get<ILogger>();
			}
		}

		public void OnException(ExceptionContext filterContext)
		{
			// Get the exception
			Exception ex = filterContext.Exception;

			// Log it.
			Log.Error(String.Format("Unhandled exception detected. Message: {0}", ex.Message), ex);
		}
	}
}