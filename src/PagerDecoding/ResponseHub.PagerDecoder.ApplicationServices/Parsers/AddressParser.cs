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

using Enivate.ResponseHub.Model.Parsers;
using Enivate.ResponseHub.Model.Addresses;

namespace Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers
{
	public class AddressParser
	{

		private const string _addressParserJsonFileKey = "AddressParserJsonFile";



		/// <summary>
		/// Defines the cross street regex value
		/// </summary>
		private const string _crossStreetRegex = "(\\s\\/[A-Za-z\\s][A-Za-z\\s-]+){1}(\\/\\/[A-Za-z][A-Za-z\\s-]+)?";

		/// <summary>
		/// Contains the data loaded from the json data file.
		/// </summary>
		private readonly AddressParserData _addressParserData;

		public AddressParser()
		{

			string jsonData;

			// Load the json data from the embedded resource file.
			using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Enivate.ResponseHub.PagerDecoder.ApplicationServices.address_parser.json")))
			{
				jsonData = reader.ReadToEnd();
			}

			// Load the json object
			_addressParserData = JsonConvert.DeserializeObject<AddressParserData>(jsonData);
		}

		public string GetAddressFromMessage(string message)
		{

			// If the string is null or emtpy, just return
			if (String.IsNullOrEmpty(message))
			{
				return String.Empty;
			}

			// Create the value to store the map ref regex index in
			int mapRefRegexIndex;

			// If there is no spatial or regex match, just return
			Match spatialMapRefRegex = Regex.Match(message, JobMessageParser.SpatialVisionRegex);
			Match melwayMapRefRegex = Regex.Match(message, JobMessageParser.MelwayRegex);
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
			if (!ContainsStreetType(message))
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
			string addressValue = message.Substring(0, mapRefRegexIndex).ToUpper();

			// If the address now ends in ' - ' after the map ref removal, get rid of the last ' - ' so we don't mess up that check later on
			if (addressValue.EndsWith(" - "))
			{
				addressValue = addressValue.Substring(0, (addressValue.Length - 3));
			}

			// If we have a "regular address type" then get that and use it. 
			// This will a street number only, and not a unit. It will use the initial index of the regex.
			Match basicAddressMatch = Regex.Match(addressValue, _addressParserData.StreetAddressRegex);
			if (basicAddressMatch.Success && basicAddressMatch.Groups.Count > 1)
			{
				addressValue = addressValue.Substring(basicAddressMatch.Groups[1].Index);

				// If we have any cross street information, get rid of that also
				if (Regex.IsMatch(addressValue, _crossStreetRegex))
				{
					addressValue = Regex.Replace(addressValue, _crossStreetRegex, "");
				}
			}
			else
			{

				// Now we want to get the last index of ' - ' which is used to separate parts of the message
				// This will be everything AFTER the last ' - ' within the message.
				int lastIndexHyphen = addressValue.LastIndexOf(" - ");

				// If there is a last index of, then chop the string at that point (+3 chars so we don't include the ' - ')
				if (lastIndexHyphen != -1)
				{
					addressValue = addressValue.Substring(lastIndexHyphen + 3);
				}

				// If we have any cross street information, get rid of that also
				if (Regex.IsMatch(addressValue, _crossStreetRegex))
				{
					addressValue = Regex.Replace(addressValue, _crossStreetRegex, "");
				}

				// If there is a "CNR", get everything after it
				int cnrLastIndex = addressValue.LastIndexOf("CNR");
				if (cnrLastIndex != -1)
				{
					addressValue = addressValue.Substring(cnrLastIndex);
				}

				// Remove anything inside brackets
				addressValue = Regex.Replace(addressValue, "\\(.*\\)", "");

				// Check to see if we have any of the job types, and if so, strip everything before and including that
				addressValue = StripJobTypes(addressValue);

				// Check to see if we have any common phrases, and if so, strip them out also
				addressValue = StripCommonExcludeWords(addressValue);

			}

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

				// Add trailing space to make sure we don't strip chars from the middle of words
				string excludeWordSpaces = String.Format("{0} ", excludeWord);

				if (addressValue.Contains(excludeWordSpaces))
				{
					addressValue = addressValue.Replace(excludeWordSpaces, " ");
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

	}
}
