﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.WindowsService.Parsers
{
	public class MessageParser
	{

		/// <summary>
		/// Spatial map reference vision regular expression pattern
		/// </summary>
		public const string SpatialVisionRegex = ".*\\s+(SVVB|SV\\s?C)\\s+(\\d{1,4})\\s+([A-Z]\\d{1,2})\\s+";

		/// <summary>
		/// Melway map reference regular expression pattern.
		/// </summary>
		public const string MelwayRegex = ".*\\s+(M|MEL)\\s+(\\d{1,3})\\s+([A-Z]\\d{1,2})\\s+";

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

		public Message ParseMessage(PagerMessage pagerMessage)
		{
			
			// Create the message 
			Message msg = new Message()
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
		private Location GetLocation(string messageContent)
		{
			
			// Create the location variable
			Location location = null;

			// First check for spatial vision match
			Match mapRefMatch = Regex.Match(messageContent, SpatialVisionRegex);
			if (mapRefMatch.Success && mapRefMatch.Groups.Count > 4)
			{
				
				// Populate the location from the map reference.
				location = PopulateLocationFromMapReference(mapRefMatch.Groups[1].Value, 
					MapType.SpatialVision, 
					Int32.Parse(mapRefMatch.Groups[3].Value), // We know this is an integer as the regex matched against digits.
					mapRefMatch.Groups[4].Value);

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
					Int32.Parse(mapRefMatch.Groups[2].Value), // We know this is an integer as the regex matched against digits. 
					mapRefMatch.Groups[3].Value);
			}


			// return the location
			return location;
		}

		private Location PopulateLocationFromMapReference(string fullMapRef, MapType mapType, int mapPage, string gridReference)
		{

			// Create the location object
			Location location = new Location()
			{
				MapReference = fullMapRef,
				MapType = mapType,
				MapPage = mapPage,
				GridReference = gridReference
			};

			// Get the coordinates from the index maps

			// return the location
			return location;

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
