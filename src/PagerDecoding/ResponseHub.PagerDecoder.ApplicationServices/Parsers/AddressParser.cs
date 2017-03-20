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

	}
}
