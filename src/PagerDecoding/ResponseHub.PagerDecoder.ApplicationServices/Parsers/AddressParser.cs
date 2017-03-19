using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Enivate.ResponseHub.Model.Parsers;
using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers
{
	public class AddressParser
	{

		private const string _addressParserJsonFileKey = "AddressParserJsonFile";

		/// <summary>
		/// Contains the data loaded from the json data file.
		/// </summary>
		private readonly AddressParserData _addressParserData;

		public AddressParser()
		{
			// Get the filepath to the JSON file
			string filePath;

			// If there is a configuration setting, use that
			if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings[_addressParserJsonFileKey]))
			{
				filePath = ConfigurationManager.AppSettings[_addressParserJsonFileKey];
			}
			else
			{
				// Load from the current execution path
				filePath = String.Format("{0}\\address_parser.json", Path.GetDirectoryName(Assembly.GetAssembly(GetType()).Location));
			}

			// If the file does not exist, throw an exception
			if (!File.Exists(filePath))
			{
				throw new Exception(String.Format("The AddressParser JSON file does not exist at path: '{0}'.", filePath));
			}

			string jsonData = File.ReadAllText(filePath);

			// Load the json object
			_addressParserData = JsonConvert.DeserializeObject<AddressParserData>(jsonData);
		}

		public string GetAddressFromString(string str)
		{

			// If the string is null or emtpy, just return
			if (String.IsNullOrEmpty(str))
			{
				return String.Empty;
			}

			// Create the value to store the map ref regex index in
			int mapRefRegexIndex;

			// If there is no spatial or regex match, just return
			Match spatialMapRefRegex = Regex.Match(str, JobMessageParser.SpatialVisionRegex);
			Match melwayMapRefRegex = Regex.Match(str, JobMessageParser.MelwayRegex);
			if (spatialMapRefRegex.Success && spatialMapRefRegex.Groups.Count > 1)
			{
				// Get the index of regex to know where we can chop off our string initially.
				mapRefRegexIndex = spatialMapRefRegex.Groups[1].Index;
			}
			else if (melwayMapRefRegex.Success && melwayMapRefRegex.Groups.Count > 1)
			{
				// Get the index of regex to know where we can chop off our string initially.
				mapRefRegexIndex = melwayMapRefRegex.Groups[1].Index;
			}
			else
			{
				return String.Empty;
			}

			// Determine if there is a road type in the address string
			if (!ContainsStreetType(str))
			{
				// No street types found, exit
				return String.Empty;
			}

			// If the mapRefRegexIndex is <= 0, we have an invalid index and we should just return
			if (mapRefRegexIndex <= 0)
			{
				return String.Empty;
			}
			// Get our initial address value
			// This will be everything BEFORE the map reference.
			// Make sure we are in upper case for all string comparisons further on.
			string addressValue = str.Substring(0, mapRefRegexIndex).ToUpper();

			// Now we want to get the last index of ' - ' which is used to separate parts of the message
			// This will be everything AFTER the last ' - ' within the message.
			int lastIndexHyphen = addressValue.LastIndexOf(" - ");

			// If there is a last index of, then chop the string at that point (+3 chars so we don't include the ' - ')
			if (lastIndexHyphen != -1)
			{
				addressValue = addressValue.Substring(lastIndexHyphen + 3);
			}

			// Check to see if we have any of the job types, and if so, strip everything before and including that
			addressValue = StripJobTypes(addressValue);

			// Check to see if we have any common phrases, and if so, strip them out also
			addressValue = StripCommonExcludeWords(addressValue);

			return addressValue.Trim();

		}

		/// <summary>
		/// Strips the common exclude words from the address value
		/// </summary>
		/// <param name="addressValue"></param>
		/// <returns></returns>
		private string StripCommonExcludeWords(string addressValue)
		{
			foreach(string excludeWord in _addressParserData.ExcludeWords)
			{
				if (addressValue.Contains(excludeWord))
				{
					addressValue = addressValue.Replace(excludeWord, "");
				}
			}

			// return the address value
			return addressValue;
		}

		/// <summary>
		/// Strips the job types from the address.
		/// </summary>
		/// <param name="addressValue"></param>
		/// <returns></returns>
		private string StripJobTypes(string addressValue)
		{
			foreach (string jobType in _addressParserData.JobTypes)
			{
				if (addressValue.Contains(jobType))
				{
					// Get the index of the job type
					int jobTypeIndex = addressValue.IndexOf(jobType);
					// If it's found, add the length of the job type onto the index.
					if (jobTypeIndex != -1)
					{
						jobTypeIndex += jobType.Length;
					}
					// Get the address value from AFTER the job type.
					addressValue = addressValue.Substring(jobTypeIndex);

					// Exit the loop, we have all that we need.
					break;
				}
			}

			return addressValue;
		}

		/// <summary>
		/// Determines if there is a street type contained within the string.
		/// </summary>
		/// <param name="str">The string to check for street types in.</param>
		/// <returns>True if a street type is found, otherwise false.</returns>
		public bool ContainsStreetType(string str)
		{
			return _addressParserData.StreetTypes.Any(i => str.Contains(i));
		}

		public StructuredAddress GetStructuredAddressFromGoogleGeocode(string geocodeJson)
		{

			// Load the geocode data into JObject for querying
			JObject geocodeData = JObject.Parse(geocodeJson);

			// If the status is not OK, return null address
			if (geocodeData["status"].ToString() != "OK")
			{ 
				return null;
			}

			// Get the address components
			JArray addressComponents = (JArray)geocodeData["results"][0]["address_components"];

			// Create the Structred Address object
			StructuredAddress address = new StructuredAddress();

			// Set the address components.
			foreach(JObject component in addressComponents)
			{
				MapAddressComponent(component, ref address);
			}

			// Set the latitude/longitude
			address.Latitude = Double.Parse(geocodeData["results"][0]["geometry"]["location"]["lat"].ToString());
			address.Longitude = Double.Parse(geocodeData["results"][0]["geometry"]["location"]["lng"].ToString());

			// Set the google geocode id
			address.GoogleGeocodeId = geocodeData["results"][0]["place_id"].ToString();
			
			// return the address
			return address;

		}

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
