using System;
using System.Collections.Generic;
using System.Configuration;	
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Enivate.ResponseHub.Model.Addresses;
using Enivate.ResponseHub.Logging;

namespace Enivate.ResponseHub.ApplicationServices.Wrappers
{
	public class GoogleGeocodeWrapper
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
		private const string _googleGeocodeApiKeyConfigKey = "GoogleGeocodeApiKey";

		public GoogleGeocodeWrapper(ILogger log)
		{

			// Set the log
			_log = log;

			// Get the API key from the configuration
			_apiKey = ConfigurationManager.AppSettings[_googleGeocodeApiKeyConfigKey];

			// If the api key is null or empty, throw exception
			if (String.IsNullOrEmpty(_apiKey))
			{
				throw new Exception("Google Geocoding API key does not exist in the configuration.");
			}
		}

		/// <summary>
		/// Gets the structured address object from the Google Geocode service.
		/// </summary>
		/// <param name="addressQuery">The address query to submit.</param>
		/// <returns></returns>
		public async Task<StructuredAddress> GetGoogleGeocodeResult(string addressQuery)
		{

			if (String.IsNullOrEmpty(addressQuery))
			{
				throw new Exception("The 'addressQuery' parameter must contain an address to search for.");
			}


			try
			{

				// Double check any spaces are removed from the query
				addressQuery = addressQuery.Replace(" ", "+");

				// Get the service url. If the API Key is available, then use that.
				string serviceUrl = String.Format("https://maps.googleapis.com/maps/api/geocode/json?address={0}&key={1}", addressQuery, _apiKey);

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

				// Log the response from Google Geocode Service
				await _log.Debug(String.Format("Found address data for address: {0}", addressQuery));

				// Deserialise the ReverseGeocodeResult object
				StructuredAddress address = GetStructuredAddressFromGoogleGeocode(responseContent, addressQuery);

				// return the result
				return address;
			}
			catch (Exception ex)
			{
				// Log The error
				await _log.Error("An error occurred requesting the Google Maps Geocode API.", ex);

				// return a null result
				return null;
			}

		}

		/// <summary>
		/// Converts the geocode json data into a structured address value.
		/// </summary>
		/// <param name="geocodeJson">The Google geocode data response.</param>
		/// <returns>The structured address for the response data</returns>
		public StructuredAddress GetStructuredAddressFromGoogleGeocode(string geocodeJson, string addressQuery)
		{

			// Load the geocode data into JObject for querying
			JObject geocodeData = JObject.Parse(geocodeJson);

			// If the status is not OK, return null address
			if (geocodeData["status"] != null && geocodeData["status"].ToString() != "OK")
			{
				return null;
			}

			// Get the address components
			JArray addressComponents = (JArray)geocodeData["results"][0]["address_components"];

			// Create the Structred Address object
			StructuredAddress address = new StructuredAddress();

			// Set the address components.
			foreach (JObject component in addressComponents)
			{
				MapAddressComponent(component, ref address);
			}

			// Set the latitude/longitude
			address.Latitude = Double.Parse(geocodeData["results"][0]["geometry"]["location"]["lat"].ToString());
			address.Longitude = Double.Parse(geocodeData["results"][0]["geometry"]["location"]["lng"].ToString());

			// Set the google geocode id
			address.GoogleGeocodeId = geocodeData["results"][0]["place_id"].ToString();

			// Set the address query hash
			address.AddressQueryHash = StructuredAddress.GetAddressQueryHash(addressQuery);

			// return the address
			return address;

		}

		/// <summary>
		/// Maps the specific address components to the strutured address object.
		/// </summary>
		/// <param name="addressComponent">The address component to map to the address object.</param>
		/// <param name="address">The address object to map the data to.</param>
		private void MapAddressComponent(JObject addressComponent, ref StructuredAddress address)
		{
			// Get the types of the address component
			IList<string> types = addressComponent["types"].Select(i => (string)i).ToList();

			// If types contains a "subpremise", set the unit number
			if (types.Contains("subpremise"))
			{
				address.Unit = addressComponent["short_name"].ToString();
			}

			// If type contains a "street_number", set the street number
			if (types.Contains("street_number"))
			{
				address.StreetNumber = addressComponent["short_name"].ToString();
			}

			// If type contains a "route", set the street
			if (types.Contains("route"))
			{
				address.Street = addressComponent["short_name"].ToString();
			}

			// If type contains a "locality", set the suburb
			if (types.Contains("locality"))
			{
				address.Suburb = addressComponent["long_name"].ToString();
			}

			// If it contains a "administrative_area_level_1", set the state
			if (types.Contains("administrative_area_level_1"))
			{
				address.State = addressComponent["short_name"].ToString();
			}

			// If it contains a "postal_code", set the postcode
			if (types.Contains("postal_code"))
			{
				address.Postcode = addressComponent["short_name"].ToString();
			}


		}

	}
}
