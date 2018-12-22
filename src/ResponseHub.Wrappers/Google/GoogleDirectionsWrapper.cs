using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Spatial;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Enivate.ResponseHub.Wrappers.Google
{
	public class GoogleDirectionsWrapper
	{
		
		/// <summary>
	 /// The logger
	 /// </summary>
		private ILogger _log;

		/// <summary>
		/// The API key for the google geocode service
		/// </summary>
		private readonly string _apiKey;

		/// <summary>
		/// The configuration key containing the google geocode api key.
		/// </summary>
		private const string _googleDirectionsApiKeyConfigKey = "GoogleDirectionsApiKey";

		public GoogleDirectionsWrapper(ILogger log)
		{


			// Set the log
			_log = log;

			// Get the API key from the configuration
			_apiKey = ConfigurationManager.AppSettings[_googleDirectionsApiKeyConfigKey];

			// If the api key is null or empty, throw exception
			if (String.IsNullOrEmpty(_apiKey))
			{
				throw new Exception("Google Directions API key does not exist in the configuration.");
			}
		}

		public async Task<DirectionsInfo> GetDirectionsCoordinates(Coordinates startLocation, Coordinates endLocation)
		{
			try
			{
				
				// Get the service url. If the API Key is available, then use that.
				string serviceUrl = String.Format("https://maps.googleapis.com/maps/api/directions/json?origin={0}&destination={1}&key={2}", 
					startLocation.ToString().Replace(" ", ""),
					endLocation.ToString().Replace(" ", ""), 
					_apiKey);

				// Create the client and request objects
				HttpClient client = new HttpClient();
				HttpResponseMessage response = await client.GetAsync(serviceUrl, HttpCompletionOption.ResponseContentRead);

				// Read the response data.
				string responseContent = await response.Content.ReadAsStringAsync();

				// if there is no json data, return failed.
				if (String.IsNullOrEmpty(responseContent))
				{

					// Log the empty Google API response.
					await _log.Warn(String.Format("Empty response returned from Google Maps API request. Requested URL: {0} | Response status: {1}", serviceUrl, response.StatusCode));

					// return a null result
					return null;
				}

				// Deserialise the ReverseGeocodeResult object
				DirectionsInfo directionsCoords = GetDirectionsFromGoogleDirections(responseContent, startLocation);

				// return the result
				return directionsCoords;
			}
			catch (Exception ex)
			{
				// Log The error
				await _log.Error("An error occurred requesting the Google Maps Directions API.", ex);

				// return a null result
				return null;
			}
		}

		/// <summary>
		/// Maps the directions api response to a list of Coordinates objects.
		/// </summary>
		/// <param name="directionsJson"></param>
		/// <param name="startLocation"></param>
		/// <returns></returns>
		public DirectionsInfo GetDirectionsFromGoogleDirections(string directionsJson, Coordinates startLocation)
		{
			// Load the directions data into JObject for querying
			JObject directionsData = JObject.Parse(directionsJson);

			// If the status is not OK, return null list
			if (directionsData["status"] != null && directionsData["status"].ToString() != "OK")
			{
				return null;
			}

			// Create the directions class
			DirectionsInfo directions = new DirectionsInfo();

			// Set the total distance
			directions.TotalDistance = Double.Parse(directionsData["routes"][0]["legs"][0]["distance"]["value"].ToString());

			// Get the route legs
			JArray routeLegSteps = (JArray)directionsData["routes"][0]["legs"][0]["steps"];

			// Add the coordinates of the directions to the DirectionsInfo class.
			foreach(JObject step in routeLegSteps)
			{
				List<Coordinates> legCoords = MapLegToCoordinates(step);
				if (legCoords != null)
				{
					directions.Coordinates.AddRange(legCoords);
				}
			}

			// return the directions
			return directions;

		}

		/// <summary>
		/// Maps a specific route leg step to a Coordinates object.
		/// </summary>
		/// <param name="step"></param>
		/// <returns></returns>
		private List<Coordinates> MapLegToCoordinates(JObject step)
		{

			try
			{

				List<Coordinates> stepCoords = new List<Coordinates>();

				// Add the points from the step
				stepCoords.AddRange(DecodeGooglePolyline(step["polyline"]["points"].ToString()));

				// return the mapped coords.
				stepCoords.Add(new Coordinates(
					Double.Parse(step["end_location"]["lat"].ToString()),
					Double.Parse(step["end_location"]["lng"].ToString())
					));

				// return the coords
				return stepCoords;

			}
			catch (Exception ex)
			{
				_log.Error(String.Format("Error creating coordinates from route leg step. Message: {0}", ex.Message), ex);
				return null;
			}
		}

		/// <summary>
		/// Decode google style polyline coordinates.
		/// </summary>
		/// <param name="encodedPoints"></param>
		/// <returns></returns>
		public static IEnumerable<Coordinates> DecodeGooglePolyline(string encodedPoints)
		{
			if (string.IsNullOrEmpty(encodedPoints))
				throw new ArgumentNullException("encodedPoints");

			char[] polylineChars = encodedPoints.ToCharArray();
			int index = 0;

			int currentLat = 0;
			int currentLng = 0;
			int next5bits;
			int sum;
			int shifter;

			while (index < polylineChars.Length)
			{
				// calculate next latitude
				sum = 0;
				shifter = 0;
				do
				{
					next5bits = (int)polylineChars[index++] - 63;
					sum |= (next5bits & 31) << shifter;
					shifter += 5;
				} while (next5bits >= 32 && index < polylineChars.Length);

				if (index >= polylineChars.Length)
					break;

				currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

				//calculate next longitude
				sum = 0;
				shifter = 0;
				do
				{
					next5bits = (int)polylineChars[index++] - 63;
					sum |= (next5bits & 31) << shifter;
					shifter += 5;
				} while (next5bits >= 32 && index < polylineChars.Length);

				if (index >= polylineChars.Length && next5bits >= 32)
					break;

				currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

				yield return new Coordinates
				{
					Latitude = Convert.ToDouble(currentLat) / 1E5,
					Longitude = Convert.ToDouble(currentLng) / 1E5
				};
			}
		}

	}
}
