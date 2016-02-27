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

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("media")]
	public class MediaController : Controller
    {

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
    }
}