using Enivate.ResponseHub.Wrappers.Google;
using Enivate.ResponseHub.Model.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Enivate.ResponseHub.UI.Controllers.Api
{
	[RoutePrefix("api/google-api")]
	public class GoogleApiController : BaseApiController
    {

		[Route("directions")]
		[HttpGet]
		public async Task<DirectionsInfo> GetDirections()
		{
			
			// Get the query string
			IEnumerable<KeyValuePair<string, string>> qs = ControllerContext.Request.GetQueryNameValuePairs();

			// If there is no start_loc and end_loc query strings, throw bad request
			if (!qs.Any(i => i.Key.ToLower() == "start_loc") || !qs.Any(i => i.Key.ToLower() == "end_loc"))
			{
				throw new HttpResponseException(HttpStatusCode.BadRequest);
			}

			// Get the start and end location values
			Coordinates startLoc;
			Coordinates endLoc;

			try
			{
				// Get the start loc coords
				string[] startLocArray = qs.First(i => i.Key.ToLower() == "start_loc").Value.Split(new char[] { ',' });
				startLoc = new Coordinates(Double.Parse(startLocArray[0]), Double.Parse(startLocArray[1]));

				// Get the start loc coords
				string[] endLocArray = qs.First(i => i.Key.ToLower() == "end_loc").Value.Split(new char[] { ',' });
				endLoc = new Coordinates(Double.Parse(endLocArray[0]), Double.Parse(endLocArray[1]));

			}
			catch (Exception ex)
			{
				// Log the exception and return bad request
				await Log.Error(String.Format("Error parsing query string values into coordinates. Message: {0}", ex.Message), ex);
				throw new HttpResponseException(HttpStatusCode.BadRequest);
			}

			// return the list of coordinates from the google geocode result
			GoogleDirectionsWrapper directions = new GoogleDirectionsWrapper(Log);
			return await directions.GetDirectionsCoordinates(startLoc, endLoc);

		}


	}
}
