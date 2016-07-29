using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Enivate.ResponseHub.Logging.Configuration;

namespace Enivate.ResponseHub.Logging
{
    public class FileLogger : ILogger
    {

		#region Public log methods

		/// <summary>
		/// Writes a debug message to the log.
		/// </summary>
		/// <param name="message">The message to write.</param>
		public async Task Debug(string message)
		{
			await AddLogEntry("DEBUG", message);
		}

		/// <summary>
		/// Writes an info message to the log.
		/// </summary>
		/// <param name="message">The message to write.</param>
		public async Task Info(string message)
		{
			await AddLogEntry("INFO", message);
		}

		/// <summary>
		/// Writes a warning message to the log.
		/// </summary>
		/// <param name="message">The message to write.</param>
		public async Task Warn(string message)
		{
			await AddLogEntry("WARN", message);
		}

		/// <summary>
		/// Writes a warning message to the log.
		/// </summary>
		/// <param name="message">The message to write.</param>
		/// <param name="ex">The exception to write to the log.</param>
		public async Task Warn(string message, Exception ex)
		{
			await AddLogEntry("WARN", message, ex);
		}

		/// <summary>
		/// Writes an error message to the log.
		/// </summary>
		/// <param name="message">The message to write.</param>
		public async Task Error(string message)
		{
			await AddLogEntry("ERROR", message);
		}

		/// <summary>
		/// Writes an error message to the log.
		/// </summary>
		/// <param name="message">The message to write.</param>
		/// <param name="ex">The exception to write to the log.</param>
		public async Task Error(string message, Exception ex)
		{
			await AddLogEntry("ERROR", message, ex);
		}

		#endregion

		#region Log write functions

		/// <summary>
		/// Adds a new item to the system log.
		/// </summary>
		/// <param name="logLevel">The level to log.</param>
		/// <param name="message">The message to log.</param>
		private async Task AddLogEntry(string logLevel, string message)
		{
			await AddLogEntry(logLevel, message, null);
		}

		/// <summary>
		/// Adds a new item to the system log.
		/// </summary>
		/// <param name="logLevel">The level to log.</param>
		/// <param name="message">The message to log.</param>
		/// <param name="exception">The exception to log.</param>
		private async Task AddLogEntry(string logLevel, string message, Exception exception)
		{

			// If the logLevel cannot be written to, then just return
			bool logCanBeWritten = CanWriteLogEntry(logLevel);

			if (!logCanBeWritten)
			{
				return;
			}

			// Create the log item
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0}  ({1}):  {2}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), logLevel.ToUpper(), message);
			sb.AppendLine();

			// Log the exception details
			GenerateExceptionLog(sb, exception);

			try
			{

				// Get the log directory
				string logDirectory = GetLogDirectory();

				// if the directory does not exist, then create it
				if (!Directory.Exists(logDirectory))
				{
					Directory.CreateDirectory(logDirectory);
				}

				// Generate the log file name
				string logFileName = String.Format("{0}\\{1}.log", logDirectory, DateTime.Now.ToString("yyyy-MM-dd"));

				// Begin using the textwrite to write the log to the file
				using (StreamWriter writer = new StreamWriter(logFileName, true, Encoding.UTF8))
				{
					await writer.WriteAsync(sb.ToString());
				}

			}
			catch (Exception ex)
			{
				// Write to the system event log
				System.Diagnostics.EventLog.WriteEntry("Application Error", ex.Message);
			}

		}

		/// <summary>
		/// Recursive function to generate the exception log information. Will recurse into InnerExceptions until no further exceptions exist.
		/// </summary>
		/// <param name="sb">The StringBuilder containing the exception log.</param>
		/// <param name="ex">The exception to log.</param>
		private void GenerateExceptionLog(StringBuilder sb, Exception ex)
		{
			if (ex != null)
			{
				// Exception message
				sb.AppendFormat("Exception: {0}", ex.Message);
				sb.AppendLine();
				sb.AppendFormat("Stack trace: {0}", ex.StackTrace);
				sb.AppendLine();
				sb.AppendLine();

				// Recurse to the inner exception
				GenerateExceptionLog(sb, ex.InnerException);
			}
		}

		/// <summary>
		/// Gets the log directory path.
		/// </summary>
		/// <returns>The log directory path, or empty string on error.</returns>
		private string GetLogDirectory()
		{
			// Get the log directory from the configuration
			string logDirectory = LoggingConfiguration.Current.LogDirectory;

			// If the logDirectory is null, return
			if (String.IsNullOrEmpty(logDirectory))
			{
				throw new Exception("Setting 'diagnostics[logDirectory]' cannot be null or empty.");
			}

			// If the log directory starts with ~, its a virtual path, so use the Server.MapPath to get full directory
			if (logDirectory[0] == '~' && HttpContext.Current != null)
			{
				logDirectory = HttpContext.Current.Server.MapPath(logDirectory);
			}
			else if (logDirectory[0] == '.')
			{
				// Log directory is a relative path (../ or ./) so append to current directory
				logDirectory = String.Format("{0}\\{1}", Environment.CurrentDirectory, logDirectory.Substring(1));
			}

			// return the log directory
			return logDirectory;
		}

		/// <summary>
		/// Determines if the log level can be written based on the configured max log level.
		/// </summary>
		/// <param name="logLevel">The log level for the entry.</param>
		/// <returns>True if the log entry can be written, otherwise false.</returns>
		private bool CanWriteLogEntry(string logLevel)
		{
			// Get the log directory from the configuration
			string configLogLevel = LoggingConfiguration.Current.LogLevel;

			// if the log level is null or empty, then default to INFO
			if (String.IsNullOrEmpty(configLogLevel))
			{
				configLogLevel = "INFO";
			}

			// Set to ToUpper
			configLogLevel = configLogLevel.ToUpper();
			logLevel = logLevel.ToUpper();

			// Check the config level based on the specified level
			if (configLogLevel == "DEBUG")
			{
				// This is the lowest info, so return true
				return true;
			}
			else if (configLogLevel == "INFO" && (logLevel == "INFO" || logLevel == "WARN" || logLevel == "ERROR"))
			{
				return true;
			}
			else if (configLogLevel == "WARN" && (logLevel == "WARN" || logLevel == "ERROR"))
			{
				return true;
			}
			else if (configLogLevel == "ERROR" && logLevel == "ERROR")
			{
				return true;
			}
			else
			{
				// Unknown log level
				return false;
			}


		}

		#endregion

	}
}
