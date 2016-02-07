using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.WindowsService.Parsers
{
	public class MessageParser
	{

		public const string EmergencyPrefix = "@@";

		public const string NonEmergencyPrefix = "Hb";

		public const string AdministrationPrefix = "QD";

		public static Message ParseMessage(PagerMessage pagerMessage)
		{
			
			// Create the message 
			Message msg = new Message()
			{
				Capcode = pagerMessage.Address,
				Timestamp = pagerMessage.Timestamp
			};

			// Now we need to get the priority
			msg.Priority = GetMessgePriority(pagerMessage.MessageContent);

			string messageBody = "";

			// We now need strip the priority prefix from the message, and set the MessageBody content of the message
			if (!String.IsNullOrEmpty(pagerMessage.MessageContent) && pagerMessage.MessageContent.Length > 2)
			{
				if (pagerMessage.MessageContent.StartsWith(EmergencyPrefix) || pagerMessage.MessageContent.StartsWith(NonEmergencyPrefix) ||
					pagerMessage.MessageContent.StartsWith(AdministrationPrefix))
				{
					messageBody = pagerMessage.MessageContent.Substring(2);
				}
				else
				{
					messageBody = pagerMessage.MessageContent;
				}
			}
			msg.MessageContent = messageBody;

			// Get the job number from the message.
			msg.JobNumber = getJobNumber(messageBody);

			// return the message
			return msg;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="messageBody"></param>
		/// <returns></returns>
		private static string getJobNumber(string messageBody)
		{
			if (String.IsNullOrEmpty(messageBody) || messageBody.Length < 8)
			{
				return "";
			}

			// Create the job number variable
			string jobNumber = "";

			Match jobNumberMatch = Regex.Match(messageBody, "(?:\\w*)?([S|F]\\d{7,})\\s*.*");

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
		private static MessagePriority GetMessgePriority(string messageContent)
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
