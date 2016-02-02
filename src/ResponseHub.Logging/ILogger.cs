using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Logging
{
	public interface ILogger
	{

		Task Debug(string message);

		Task Info(string message);

		Task Warn(string message);

		Task Warn(string message, Exception ex);

		Task Error(string message);

		Task Error(string message, Exception ex);

	}
}
