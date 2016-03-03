using Enivate.ResponseHub.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers
{
	public class PagerMessageParser
	{

		private const string PocsagPagerMessageRegex = @"^(\d{7,})\s+(\d{2}:\d{2}:\d{2})\s+(\d{2}-\d{2}-\d{2})\s+(POCSAG-\d)\s+([A-Z]*)\s+(\d{3,4})\s+([^\s].*)$";

		/// <summary>
		/// The log writer.
		/// </summary>
		private ILogger _log;

		public PagerMessageParser(ILogger log)
		{
			_log = log;
		}

		/// <summary>
		/// Parse the pager message from the raw pager message.
		/// </summary>
		/// <param name="rawMessage"></param>
		/// <returns></returns>
		public PagerMessage ParsePagerMessage(string rawMessage)
		{

			// If the rawMessage is null or empty, then throw argument null exception
			if (String.IsNullOrEmpty(rawMessage))
			{
				throw new ArgumentNullException("rawMessage");
			}

			// Ensure there is correct amount of data in the message based on a regex match
			Match match = Regex.Match(rawMessage, PocsagPagerMessageRegex);

			// If there is a match, and there are 8 groups (7 data groups plus initial matching group) then parse the message, otherwise throw invalid format exception
			if (match.Success && match.Groups.Count == 8)
			{
				PagerMessage pagerMessage = GetPagerMessageFromRegexMatch(match);
				return pagerMessage;
			}
			else
			{
				_log.Warn(String.Format("The raw message is not in the correct format. Format needs to be: '[PagerCapcode] [Time] [Date] [Mode] [Type] [Bitrate] [Message]'\r\n\t\tRaw message: {0}", rawMessage.Trim()));
				return null;
			}

		}

		/// <summary>
		/// Gets the pager message from the regex match. 
		/// </summary>
		/// <param name="match">The regex pager match to get the message from.</param>
		/// <returns>The parsed pager message.</returns>
		private PagerMessage GetPagerMessageFromRegexMatch(Match match)
		{

			// Ensure the match is valid.
			if (match == null)
			{
				throw new ArgumentNullException("match");
			}

			// Get the details of the pager message
			string address = match.Groups[1].Value;
			DateTime timestamp = DateTime.ParseExact(String.Format("{0} {1}", match.Groups[3].Value, match.Groups[2].Value), "dd-MM-yy HH:mm:ss", CultureInfo.InvariantCulture);
			string mode = match.Groups[4].Value;
			string type = match.Groups[5].Value;
			int bitrate = Int32.Parse(match.Groups[6].Value);
			string messageContent = match.Groups[7].Value;

			// calculate the message hash
			string messageHash = HashGenerator.GetSha1HashString(String.Format("{0}_{1}_{2}_{3}", address, match.Groups[3].Value, match.Groups[2].Value, messageContent), 1);

			// Create the pager message object
			PagerMessage pagerMessage = new PagerMessage()
			{
				Address = address,
				Bitrate = bitrate,
				Timestamp = timestamp,
				Mode = mode,
				Type = type,
				MessageContent = messageContent,
				ShaHash = messageHash
			};

			// return the pager message
			return pagerMessage;
		}
	}
}
