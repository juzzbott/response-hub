﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			}
			msg.MessageContent = messageBody;

			// Get the job number from the message.

			// return the message
			return msg;
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
