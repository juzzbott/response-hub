using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Constants;
using System.Net.Http;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Attachments.Interface;
using Enivate.ResponseHub.Model.Attachments;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Messages;
using System.Text.RegularExpressions;
using Enivate.ResponseHub.Model.WeatherData.Interface;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("media")]
	public class MediaController : BaseController
    {

		protected readonly IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();
		protected readonly IAttachmentService AttachmentService = ServiceLocator.Get<IAttachmentService>();
		protected readonly IWeatherDataService WeatherDataService = ServiceLocator.Get<IWeatherDataService>();

		[AllowAnonymous]
		[Route("mapbox-static/{lat:double},{lng:double},{zoom:int}/{size}")]
		// GET: Media/mapbox-static
		public async Task<ActionResult> MapBoxStatic(double lat, double lng, int zoom, string size)
        {

			// If the latitude is not between -90 and 90, lng is not between -180 and 180 and the zoom is not between 1 and 20, throw bad request
			if (lat > 90 || lat < -90)
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "The 'lat' parameter must be between -90 and 90 (inclusive).");
			}
			if (lng > 180 || lng < -180)
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "The 'lng' parameter must be between -180 and 180 (inclusive).");
			}
			if (zoom > 20 || zoom < 1)
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "The 'zoom' parameter must be between 1 and 20 (inclusive).");
			}

			// Get the static maps image path
			string staticImagePath = ConfigurationManager.AppSettings[ConfigurationKeys.StaticMapImagesPath];
			if (String.IsNullOrEmpty(staticImagePath))
			{
				throw new ApplicationException("The 'staticMapImagesPath' configuration attribute is null or empty.");
			}
			// Map server path if required
			if (staticImagePath[0] == '~')
			{
				staticImagePath = Server.MapPath(staticImagePath);
			}

			int latFloor = (int)lat;

			// Create the image filename
			string imageFilename = String.Format("{0}\\{1}\\{2}_{3}_{4}_{5}.png", staticImagePath, latFloor, lat, lng, zoom, size);

			// If the file does not exist, we need to get it from the google maps static api and save to disc
			if (!System.IO.File.Exists(imageFilename))
			{

				string mapBoxImageUrl = String.Format("https://api.mapbox.com/v4/juzzbott.mn25f8nc/{0},{1},{2}/{3}.png64?access_token=pk.eyJ1IjoianV6emJvdHQiLCJhIjoiMDlmN2JlMzMxMWI2YmNmNGY2NjFkZGFiYTFiZWVmNTQifQ.iKlZsVrsih0VuiUCzLZ1Lg",
					lng,
					lat,
					zoom,
					size);

				// Create the client and request objects
				HttpClient client = new HttpClient();
				HttpResponseMessage response = await client.GetAsync(mapBoxImageUrl, HttpCompletionOption.ResponseContentRead);
				byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();

				using (MemoryStream ms = new MemoryStream(responseBytes)) { 

					// Ensure the directory exists
					if (!Directory.Exists(Path.GetDirectoryName(imageFilename)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(imageFilename));
					}

					// Write the file to disk
					using (FileStream fs = new FileStream(imageFilename, FileMode.Create))
					{
						await ms.CopyToAsync(fs);
					}
				}

			}

			// return the image file.
			return File(imageFilename, "image/png");
        }

		[Route("attachment/{id}")]
		public async Task<ActionResult> DownloadAttachment(Guid id)
		{
			try
			{

				// Get the attachment based on the id
				Attachment attachment = await AttachmentService.GetAttachmentById(id, true);

				// If the attachment is not found, throw 404
				if (attachment == null)
				{
					throw new HttpException(404, "The file could not be found.");
				}

				// If it's an image file type, add the cache control header to public
				string ext = Path.GetExtension(attachment.Filename).ToLower();
				if (GeneralConstants.ImageExtensions.Contains(ext))
				{
					Response.Cache.SetCacheability(HttpCacheability.Public);
					Response.Cache.SetExpires(DateTime.Now.AddDays(7));
				}
				// return the file as a download
				return File(attachment.FileData, attachment.MimeType, attachment.Filename);

			}
			catch (Exception ex)
			{
				// Log the exception, throw 500 server error
				await Log.Error(String.Format("Error getting full attachment for download. Message: {0}", ex.Message), ex);
				throw new HttpException(500, "Internal server error");
			}
		}

		[Route("attachment-thumb/{id}")]
		public async Task<ActionResult> DownloadAttachmentThumbnail(Guid id)
		{
			try
			{

				// Get the attachment based on the id
				Attachment attachment = await AttachmentService.GetAttachmentById(id, false);

				// If the attachment is not found, throw 404
				if (attachment == null)
				{
					throw new HttpException(404, "The file could not be found.");
				}

				// Get the thumbnail image
				byte[] thumbnailData = await AttachmentService.GetResizedImage(attachment, 400, 125, false);

				// return the file as a download
				return File(thumbnailData, attachment.MimeType);

			}
			catch (Exception ex)
			{
				// Log the exception, throw 500 server error
				await Log.Error(String.Format("Error getting thumbnail attachment for download. Message: {0}", ex.Message), ex);
				throw new HttpException(500, "Internal server error");
			}
		}

		[Route("attachment-thumb-crop/{id}")]
		public async Task<ActionResult> DownloadAttachmentThumbnailCropped(Guid id)
		{
			try
			{

				// Get the attachment based on the id
				Attachment attachment = await AttachmentService.GetAttachmentById(id, false);

				// If the attachment is not found, throw 404
				if (attachment == null)
				{
					throw new HttpException(404, "The file could not be found.");
				}

				// Get the thumbnail image
				byte[] thumbnailData = await AttachmentService.GetResizedImage(attachment, 400, 300, true);

				// return the file as a download
				return File(thumbnailData, attachment.MimeType);

			}
			catch (Exception ex)
			{
				// Log the exception, throw 500 server error
				await Log.Error(String.Format("Error getting thumbnail attachment for download. Message: {0}", ex.Message), ex);
				throw new HttpException(500, "Internal server error");
			}
		}

		[Route("attachment-resized/{id:guid}")]
		public async Task<ActionResult> DownloadAttachmentResized(Guid id)
		{
			try
			{

				// Get the attachment based on the id
				Attachment attachment = await AttachmentService.GetAttachmentById(id, false);

				// If the attachment is not found, throw 404
				if (attachment == null)
				{
					throw new HttpException(404, "The file could not be found.");
				}

				int width;
				int height;

				try
				{
					width = Int32.Parse(Request.QueryString["w"]);
					height = Int32.Parse(Request.QueryString["h"]);
				}
				catch (Exception ex)
				{
					await Log.Error(String.Format("Unable to get width and height from query string. Message: {0}", ex.Message), ex);
					throw new HttpException((int)HttpStatusCode.BadRequest, "Invalid querystring data.");
				}

				// Get the thumbnail image
				byte[] thumbnailData = await AttachmentService.GetResizedImage(attachment, width, height, false);

				// return the file as a download
				return File(thumbnailData, attachment.MimeType);

			}
			catch (Exception ex)
			{
				// Log the exception, throw 500 server error
				await Log.Error(String.Format("Error getting thumbnail attachment for download. Message: {0}", ex.Message), ex);
				throw new HttpException(500, "Internal server error");
			}
		}

		[Route("job-attachments/{jobMessageId}")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> PostJobAttachment(Guid jobMessageId)
		{
			try
			{

				// Loop through the uploaded files
				foreach (string file in Request.Files)
				{

					// Get the file upload.
					HttpPostedFileBase upload = Request.Files[file];

					// Ensure there is content
					if (!String.IsNullOrEmpty(upload.FileName) && upload.ContentLength > 0)
					{

						// Get the max file size
						long maxFileSize = 104857600;
						Int64.TryParse(ConfigurationManager.AppSettings["Attachment.MaxFileSize"], out maxFileSize);

						if (upload.ContentLength > maxFileSize)
						{
							return Json(new { success = false, message = "Maximum filesize limit exceeded." });
						}

						// Ensure valuid file extentions
						if (upload.FileName.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase) ||
						upload.FileName.EndsWith(".msi", StringComparison.CurrentCultureIgnoreCase) ||
						upload.FileName.EndsWith(".bat", StringComparison.CurrentCultureIgnoreCase) ||
						upload.FileName.EndsWith(".cmd", StringComparison.CurrentCultureIgnoreCase) ||
						upload.FileName.EndsWith(".ps1", StringComparison.CurrentCultureIgnoreCase) ||
						upload.FileName.EndsWith(".sh", StringComparison.CurrentCultureIgnoreCase))
						{
							return Json(new { success = false, message = "File type not allowed." });
						}

						// Get the mime type based on the filename
						string mimeType = MimeMapping.GetMimeMapping(upload.FileName);

						// Add the file to the attachments repository
						Attachment attachment = await AttachmentService.SaveAttachment(upload.FileName, UserId, upload.InputStream, mimeType);

						// Now that we have stored the attachment, we need to add the id of the attachment to "Attachments" list on the job message
						if (attachment != null)
						{

							// Add the photo to the place
							await JobMessageService.AddAttachmentToJob(jobMessageId, attachment.Id);

							// return the json result
							return Json(new { success = true, id = attachment.Id, jobId = jobMessageId, filename = upload.FileName });

						}
						else
						{
							// return the json result
							return Json(new { success = false });
						}

					}

				}

				// return an error 
				return Json(new { success = false, message = "Unable to upload file." });

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Error uploading attachment file. Message: {0}", ex.Message), ex);
				return Json(new { success = false, message = "Unable to upload file." });
			}

		}

		[Route("download-job-attachments/{jobMessageId}")]
		public async Task<ActionResult> DownloadJobAttachments(Guid jobMessageId)
		{

			// Get the job
			JobMessage job = await JobMessageService.GetById(jobMessageId);

			// If the job is null, return 404
			if (job == null)
			{
				throw new HttpException(404, "The requested page cannot be found.");
			}

			// Get the byte archive for the attachments
			byte[] zipArchive = await AttachmentService.GetAllJobAttachments(job);

			// If the archive is null or 0 in length, throw 404
			if (zipArchive == null || zipArchive.Length == 0)
			{
				throw new HttpException(404, "The requested page cannot be found.");
			}

			string jobDownloadName = String.Format("{0}_attachments.zip", job.JobNumber);

			return File(zipArchive, "application/zip, application/octet-stream", jobDownloadName);

		}

		#region WeatherData 

		[Route("weather-data/radar-image/{filename}")]
		public ActionResult RadarImage(string filename)
		{
			// If the radar image is null or empty, then throw exception
			if (String.IsNullOrEmpty(filename))
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "Media not found.");
			}

			// Get the radar image bytes
			byte[] radarImageBytes = WeatherDataService.GetRadarImageBytes(filename);

			// return the file.
			return File(radarImageBytes, "image/png");
		}

		#endregion

	}
}
