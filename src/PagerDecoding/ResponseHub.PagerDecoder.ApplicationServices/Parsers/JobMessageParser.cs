using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;

namespace Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers
{
	public class JobMessageParser
	{

		/// <summary>
		/// Spatial map reference vision regular expression pattern
		/// </summary>
		public const string SpatialVisionRegex = ".*\\s+((SVVB|SVVB C|SV\\s?C)\\s+(\\d{1,4}[A-Z]?)\\s+([A-Z]\\d{1,2})\\s*(\\(\\d{6}\\))?)\\s+";

		/// <summary>
		/// Melway map reference regular expression pattern.
		/// </summary>
		public const string MelwayRegex = ".*\\s+((M|MEL)\\s+(\\d{1,3}[A-Z]?)\\s+([A-Z]\\d{1,2}))\\s+";

		/// <summary>
		/// Character code prefix for Emergency pager messages.
		/// </summary>
		public const string EmergencyPrefix = "@@";

		/// <summary>
		/// Character code prefix for Non-Emergency pager messages.
		/// </summary>
		public const string NonEmergencyPrefix = "Hb";

		/// <summary>
		/// Character code prefix for Administration pager messages.
		/// </summary>
		public const string AdministrationPrefix = "QD";

		/// <summary>
		/// The map index repository
		/// </summary>
		private IMapIndexRepository _repository;

		/// <summary>
		/// The log writer.
		/// </summary>
		private ILogger _log;

		/// <summary>
		/// Creates a new instance of the JobMessageParser class.
		/// </summary>
		/// <param name="repository"></param>
		public JobMessageParser(IMapIndexRepository repository, ILogger log)
		{
			_repository = repository;
			_log = log;
		}

		/// <summary>
		/// Parses the job message from the pager message.
		/// </summary>
		/// <param name="pagerMessage">The pager message to parse into a JobMessage object.</param>
		/// <returns>The job message object.</returns>
		public JobMessage ParseMessage(PagerMessage pagerMessage)
		{
			
			// Create the message 
			JobMessage msg = new JobMessage()
			{
				Capcode = pagerMessage.Address,
				Timestamp = pagerMessage.Timestamp
			};

			// Now we need to get the priority
			// We must call the GetMessagePriority method before the GetMessageBody method GetMessageBody will strip the priority prefix characters.
			msg.Priority = GetMessgePriority(pagerMessage.MessageContent);

			// Get the message body
			msg.MessageContent = GetMessageBody(pagerMessage.MessageContent);

			// Get the job number from the message.
			msg.JobNumber = GetJobNumber(msg.MessageContent);

			// Get any map references from the message
			msg.Location = GetLocation(msg.MessageContent);

			// return the message
			return msg;
		}

		/// <summary>
		/// Gets the message body from the pager content. This method strips the priority characters from the message body.
		/// </summary>
		/// <param name="messageContent">The content of the pager message.</param>
		/// <returns></returns>
		public string GetMessageBody(string pagerMessage)
		{
			string messageBody = "";

			// We now need strip the priority prefix from the message, and set the MessageBody content of the message
			if (!String.IsNullOrEmpty(pagerMessage) && pagerMessage.Length > 2)
			{
				// Strip the priority characters from the message if they exist, and set the message
				if (pagerMessage.StartsWith(EmergencyPrefix) || pagerMessage.StartsWith(NonEmergencyPrefix) ||
					pagerMessage.StartsWith(AdministrationPrefix))
				{
					messageBody = pagerMessage.Substring(2);
				}
				else
				{
					messageBody = pagerMessage;
				}
			}
			return messageBody;
		}

		/// <summary>
		/// Gets a map reference from the pager message if one exists
		/// </summary>
		/// <param name="messageContent">The content of the message to parse for a map coordinate.</param>
		/// <returns></returns>
		private LocationInfo GetLocation(string messageContent)
		{
			
			// Create the location variable
			LocationInfo location = null;

			// First check for spatial vision match
			Match mapRefMatch = Regex.Match(messageContent, SpatialVisionRegex);
			if (mapRefMatch.Success && mapRefMatch.Groups.Count > 4)
			{
				
				// Populate the location from the map reference.
				location = PopulateLocationFromMapReference(mapRefMatch.Groups[1].Value, 
					MapType.SpatialVision, 
					mapRefMatch.Groups[3].Value, 
					mapRefMatch.Groups[4].Value,
					mapRefMatch.Groups[5] != null ? mapRefMatch.Groups[5].Value : "");

				// return the location
				return location;

			}

			// Next we try and match Melway references
			mapRefMatch = Regex.Match(messageContent, MelwayRegex);
			if (mapRefMatch.Success && mapRefMatch.Groups.Count > 4)
			{
				// Populate the location from the map reference.
				location = PopulateLocationFromMapReference(mapRefMatch.Groups[1].Value, 
					MapType.Melway, 
					mapRefMatch.Groups[3].Value, 
					mapRefMatch.Groups[4].Value,
					"");
			}


			// return the location
			return location;
		}

		/// <summary>
		/// Populates the location object based on the values found in the message body.
		/// </summary>
		/// <param name="fullMapRef">The full map reference value.</param>
		/// <param name="mapType">The type of map the map reference relates to.</param>
		/// <param name="mapPage">The page number of the map reference.</param>
		/// <param name="gridSquare">THe grid reference on the page of the map reference (e.g. A1, B5 etc).</param>
		/// <returns>The location from the pager message details.</returns>
		private LocationInfo PopulateLocationFromMapReference(string fullMapRef, MapType mapType, string mapPage, string gridSquare, string gridRef)
		{

			// Get just the numbers from the precision coordinate
			if (!String.IsNullOrEmpty(gridRef))
			{
				gridRef = gridRef.Replace("(", "").Replace(")", "");
			}

			// Create the location object
			LocationInfo location = new LocationInfo()
			{
				MapReference = fullMapRef,
				MapType = mapType,
				MapPage = mapPage,
				GridSquare = gridSquare,
				GridReference = gridRef
			};

			// Get the coordinates from the index maps
			MapIndex mapIndex = GetMapIndex(mapType, mapPage);

			if (mapIndex != null)
			{

				// get the grid reference from the map index.
				MapGridReferenceInfo mapGridRefFromIndex = mapIndex.GridReferences.FirstOrDefault(i => i.GridSquare.ToLower() == gridSquare.ToLower());

				// Set the properties of the location
				if (mapGridRefFromIndex != null)
				{
					location.Coordinates = new Coordinates(mapGridRefFromIndex.Latitude, mapGridRefFromIndex.Longitude);
					
					// If we get a coordinates, and we get a 6 figure grid ref, then we want to get the more precise coordinate for the location
					if (!String.IsNullOrEmpty(gridRef))
					{
						location.Coordinates = SpatialUtility.GetCoordinatesFromGridReference(location.Coordinates, gridRef, mapIndex.Scale);
					}
				}

			}

			// return the location
			return location;

		}
		
		/// <summary>
		/// Gets the map index from the cache. If the cache object doesn't exist, then load it from the database.
		/// </summary>
		/// <param name="mapType">The map type to get the index for.</param>
		/// <param name="mapPage">The map page to get the index for.</param>
		/// <returns>The map index for the specified page number and type.</returns>
		private MapIndex GetMapIndex(MapType mapType, string mapPage)
		{
			// Get the map index from the cache
			MapIndex mapIndex = MapReferenceCache.Instance.GetCacheItem(mapType, mapPage);

			// If the map index is null, get from the database
			if (mapIndex == null)
			{

				// Get from the repository.
				mapIndex = null;

				Task.Run(async () =>
				{
					mapIndex = await _repository.GetMapIndexByPageNumber(mapType, mapPage);
				}).Wait();

				// if the mapIndex exists, add to the cache for next time
				if (mapIndex != null)
				{
					MapReferenceCache.Instance.AddMapReference(mapType, mapIndex);
				}

			}

			// return the map index
			return mapIndex;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="messageBody"></param>
		/// <returns></returns>
		public string GetJobNumber(string messageBody)
		{
			if (String.IsNullOrEmpty(messageBody) || messageBody.Length < 8)
			{
				return "";
			}

			// Create the job number variable
			string jobNumber = "";

			Match jobNumberMatch = Regex.Match(messageBody, "(?:\\w*)?([S|F]\\d{7,})\\s*.*", RegexOptions.IgnoreCase);

			// If there is a match, then get the second group (first match is entire string)
			if (jobNumberMatch.Success && jobNumberMatch.Groups.Count > 1)
			{
				jobNumber = jobNumberMatch.Groups[1].Value;
			}

			// return the job number
			return jobNumber;

		}

		/// <summary>
		/// Gets the message priority from the prefix of the message.
		/// </summary>
		/// <param name="messageContent">The content of the pager message to get the priority from.</param>
		/// <returns>The message priority.</returns>
		public MessagePriority GetMessgePriority(string messageContent)
		{
			// If the message content is not at least 2 chars, default to administration
			if (messageContent.Length < 2)
			{
				return MessagePriority.Administration;
			}

			// Get the first two letters of the message content
			string prefix = messageContent.Substring(0, 2);

			// Switch the prefix to get the priority type
			switch (prefix)
			{
				case EmergencyPrefix:
					return MessagePriority.Emergency;

				case NonEmergencyPrefix:
					return MessagePriority.NonEmergency;

				default:
					return MessagePriority.Administration;
			}
		}
	}
}
