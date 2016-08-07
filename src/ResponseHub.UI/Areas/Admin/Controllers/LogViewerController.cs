using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Logging.Configuration;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.UI.Areas.Admin.Models.Logs;
using System.Net.Mime;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("log-viewer")]
	[ClaimsAuthorize(Roles = RoleTypes.SystemAdministrator)]
	public class LogViewerController : Controller
    {

		// GET: Admin/Logs
		[Route]
		public ActionResult Index()
        {

			// Create the model
			LogViewModel model = new LogViewModel();
			model.LogFilenames = GetLogFilenames();

			// If there are log files, then set the values
			if (model.LogFilenames != null && model.LogFilenames.Count > 0)
			{

				// Get the file name for the log file
				model.SelectedFile = model.LogFilenames.First().Value;

				// If there is a query string, override it
				if (!String.IsNullOrEmpty(Request.QueryString["file"]))
				{
					// Get the file name from the query string
					string qsFile = Request.QueryString["file"];

					// Ensure there is no directory separator chars. If there are, throw bad request
					if (qsFile.IndexOf('/') != -1 || qsFile.IndexOf('\\') != -1)
					{
						throw new HttpException(400, "Invalid filename in url.");
					}

					model.SelectedFile = qsFile;
				}

				// Get the full path to the file
				string logFilePath = Server.MapPath(String.Format("{0}\\{1}", LoggingConfiguration.Current.LogDirectory, model.SelectedFile));

				// if the file exists, read it into the model
				if (System.IO.File.Exists(logFilePath))
				{
					using (StreamReader reader = new StreamReader(logFilePath))
					{
						model.LogFileData = reader.ReadToEnd();
					}
				}

				// Replace the new line chars with newline and <br />
				//model.LogFileData = model.LogFileData.Replace("\r\n", "<br />\r\n");
				//model.LogFileData = model.LogFileData.Replace("\t", "&nbsp;&nbsp;&nbsp;");

			}

			// return the model
            return View(model);
        }

		[Route("download")]
		public ActionResult Download()
		{

			string file = "";
			// If there is a query string, override it
			if (!String.IsNullOrEmpty(Request.QueryString["file"]))
			{
				// Get the file name from the query string
				file = Request.QueryString["file"];

				// Ensure there is no directory separator chars. If there are, throw bad request
				if (file.IndexOf('/') != -1 || file.IndexOf('\\') != -1)
				{
					throw new HttpException(400, "Invalid filename in url.");
				}
			}
			else
			{
				throw new HttpException(400, "Invalid query string parameters.");
			}

			// Get the full path to the file
			string logFilePath = Server.MapPath(String.Format("{0}\\{1}", LoggingConfiguration.Current.LogDirectory, file));

			// if the file exists, read it into the model
			if (System.IO.File.Exists(logFilePath))
			{
				ContentDisposition cd = new ContentDisposition()
				{
					FileName = file,
					Inline = false
				};
				Response.AppendHeader("Content-Disposition", cd.ToString());
				return File(logFilePath, "text/plain", file);
			}
			else
			{
				throw new HttpException(404, "THe log file could not be found.");
			}
		}

		/// <summary>
		/// Gets the log files in the current configured log directory.
		/// </summary>
		/// <returns></returns>
		private IList<SelectListItem> GetLogFilenames()
		{
			// Get the log directory
			string logDir = Server.MapPath(LoggingConfiguration.Current.LogDirectory);

			// Create the file paths list
			IList<string> filePaths = new List<string>();

			// Get the files in the directory
			if (Directory.Exists(logDir))
			{
				filePaths = Directory.GetFiles(logDir);
			}
			
			// reverse the list so that the newest files are first
			filePaths = filePaths.Reverse().ToList();

			// Create the list of file names (not the full paths)
			IList<SelectListItem> fileNames = new List<SelectListItem>();

			// Loop through the file paths, and get only the file names to add to the list
			foreach(string filePath in filePaths)
			{
				// Get the file info
				FileInfo fileInfo = new FileInfo(filePath);

				// If the file does not end in .log, exit
				if (fileInfo.Extension.ToLower() != ".log")
				{
					continue;
				}

				fileNames.Add(new SelectListItem() { Text = fileInfo.Name, Value = fileInfo.Name });
			}

			// return the log file names
			return fileNames;

		}

    }
}