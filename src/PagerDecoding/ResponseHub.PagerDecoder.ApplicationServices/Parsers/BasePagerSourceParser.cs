using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Addresses.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers
{
	public abstract class BasePagerSourceParser
	{

		/// <summary>
		/// The configuration key for the last message file.
		/// </summary>
		protected string LastMessageFileKey = "LastMessageFile";

		/// <summary>
		/// Contains the list of pager messages to submit.
		/// </summary>
		protected List<PagerMessage> PagerMessagesToSubmit { get; set; }

		/// <summary>
		/// Contains the list of parsed job messages to submit.
		/// </summary>
		protected Dictionary<string, JobMessage> JobMessagesToSubmit { get; set; }
		protected IJobMessageService JobMessageService { get => jobMessageService; set => jobMessageService = value; }

		/// <summary>
		/// The PagerMessage parser.
		/// </summary>
		protected PagerMessageParser PagerMessageParser;

		/// <summary>
		/// The JobMessage pager.
		/// </summary>
		protected JobMessageParser JobMessageParser;
		
		/// <summary>
		/// The last inserted message hash.
		/// </summary>
		protected string LastInsertedMessageSha = "";
		
		/// <summary>
		/// The log writer.
		/// </summary>
		protected ILogger Log;

		/// <summary>
		/// The repository for managing map indexes.
		/// </summary>
		protected IMapIndexRepository MapIndexRepository;

		/// <summary>
		/// The decoder status repository
		/// </summary>
		protected IDecoderStatusRepository DecoderStatusRepository;

		/// <summary>
		/// The job message service interface.
		/// </summary>
		private IJobMessageService jobMessageService;

		/// <summary>
		/// The address message service interface.
		/// </summary>
		protected IAddressService AddressService;

		public abstract void GetLatestMessages();

		/// <summary>
		/// Parse the pager messages into JobMessages.
		/// </summary>
		protected void ParsePagerMessagesToJobMessages()
		{
			// Loop through the pager messages
			foreach (PagerMessage pagerMessage in PagerMessagesToSubmit)
			{

				try
				{

					Task t = Task.Run(async () =>
					{

						// Get the job message from the pager message
						JobMessage jobMessage = await JobMessageParser.ParseMessage(pagerMessage);

						// Add the job to the list job messages to submit
						JobMessagesToSubmit.Add(pagerMessage.ShaHash, jobMessage);

					});

					// wait on the task to complete.
					t.Wait();

				}
				catch (Exception ex)
				{
					// Log the error the log file
					Log.Error(String.Format("Unable to parse pager message. Message: {0}", ex.Message), ex);
					Log.Error(String.Format("Pager message that failed: {0}", pagerMessage.ToString()));
				}

			}
		}

		/// <summary>
		/// Parses the list of pager messages to job messages and submits the job messages to the database.
		/// </summary>
		protected void ParseAndSubmitJobMessages()
		{
			// Parse the pager messages into job messages
			ParsePagerMessagesToJobMessages();

			// Submit the messages
			Task t = Task.Run(async () =>
			{

				// Get the list of key value pair of job numbers and capcodes from the list of messages to submit
                // HACK: Fix this
				IList<KeyValuePair<string, string>> capcodeJobNumbers = JobMessagesToSubmit
				.Where(i => !String.IsNullOrEmpty(i.Value.JobNumber))
				.Select(i => new KeyValuePair<string, string>(i.Value.Capcodes.First().Capcode, i.Value.JobNumber))
				.ToList();

				// Now we need to get the messages that 
				IList<KeyValuePair<string, Guid>> existingMessages = await JobMessageService.GetJobMessageIdsFromCapcodeJobNumbers(capcodeJobNumbers);

				// Create a list to store the update messages in
				IList<KeyValuePair<Guid, AdditionalMessage>> updateMessages = new List<KeyValuePair<Guid, AdditionalMessage>>();

				// For each of the existng messages, add the additional messages to them
				foreach(KeyValuePair<string, Guid> existingMessage in existingMessages)
				{
					// Get the messages for that job number
					IList<JobMessage> jobMessages = JobMessagesToSubmit.Where(i => i.Value.JobNumber == existingMessage.Key).Select(i => i.Value).ToList();

					// For each of the results, add the message to the additional message list
					foreach(JobMessage jobMessage in jobMessages)
					{

						// Create the additional message object
						AdditionalMessage additionalMessage = new AdditionalMessage()
						{
							MessageContent = jobMessage.MessageContent,
							Timestamp = jobMessage.Timestamp
						};

						updateMessages.Add(new KeyValuePair<Guid, AdditionalMessage>(existingMessage.Value, additionalMessage));
					}
				}

				// Submit the additional messages to the database
				await JobMessageService.AddAdditionalMessages(updateMessages);

				// Get new messages only to add
				IList<string> existingJobNumbers = existingMessages.Select(i => i.Key).Distinct().ToList();
				IList<JobMessage> newJobMessages = JobMessagesToSubmit
					.Where(i => !existingJobNumbers.Contains(i.Value.JobNumber))
					.Select(i => i.Value)
					.ToList();

				// Submit any new messages to the database
				await JobMessageService.AddMessages(newJobMessages);
			});
			t.Wait();

			if (PagerMessagesToSubmit.Count > 0)
			{
				// Get the last message sha
				// However, we need to get the 'First' message in the list, because we reverse the entries in the file I.e. most recent message is the first in the list.
				// We actually want the last message sha to be the first in the processed list so that we can ensure we only add additional messages.
				LastInsertedMessageSha = PagerMessagesToSubmit.First().ShaHash;

				// Write the last message sha to the web service
				WriteLastMessageSha(LastInsertedMessageSha);
			}
		}

		#region Invalid message checking

		/// <summary>
		/// Determines if the message appears to be invalid or not.
		/// </summary>
		/// <param name="message">The message to check.</param>
		/// <returns>True if the message appears to be invaid</returns>
		public bool MessageAppearsInvalid(string message)
		{

			// If the message does not start with one of the message qualifiers, then flag as invalid
			if (!message.StartsWith("@@") && !message.StartsWith("QD") && !message.StartsWith("Hb"))
			{
				return true;
			}

			// If the message contains some funky character sequences, then flag as invalid
			if (Regex.IsMatch(message, ".*(?:\\?{3,})|(?:\\w\\*\\w\\*\\w\\*)|(?:\\?\\)|(?:\\)\\?)).*"))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Determines if the message should be skipped based on it's contents.
		/// </summary>
		/// <param name="message">The message to check.</param>
		/// <returns>True if the message should be skipped (it's an internal system message and not a pager message).</returns>
		protected bool ShouldSkipMessage(string message)
		{
			// If the message is null or empty, continue
			if (String.IsNullOrEmpty(message))
			{
				return true;
			}

			// Skip non-required messages.
			if (message.ToUpper().Contains(")E51TIMEUPDATE"))
			{
				Log.Debug("Skipping E51TIMEUPDATE time update message.");
				return true;
			}

			if (message.ToUpper().Contains(")&)E51ASN"))
			{
				Log.Debug("Skipping E51ASN message.");
				return true;
			}

			if (message.ToLower().Contains("12 minute network heartbeat"))
			{
				Log.Debug("Skipping 12 minute heartbeat message");
				return true;
			}

			// If it's a SEGMENT messge, skip it
			string segmentsRegex = ".*(segment\\s?\\d{2}?\\s?segment\\s?\\d{2}?).*";
			if (Regex.IsMatch(message, segmentsRegex, RegexOptions.IgnoreCase))
			{
				Log.Debug("Skipping segments message.");
				return true;
			}

			// No need to skip the message, so return false
			return false;
		}

		#endregion

		#region Last Message Inserted Helpers

		/// <summary>
		/// Gets the path to the file of the last message inserted. 
		/// </summary>
		/// <returns>The last message inserted file path.</returns>
		protected string GetLastMessageFilePath()
		{
			// Get the path from the confirguration
			string path = ConfigurationManager.AppSettings[LastMessageFileKey];

			// If the path is null or empty, throw exception
			if (String.IsNullOrEmpty(path))
			{
				throw new ApplicationException(String.Format("The configuration key '{0}' is missing or is empty.", LastMessageFileKey));
			}

			// If it begins with a '.', map to curent directory without first .
			if (path[0] == '.')
			{
				path = String.Format("{0}{1}", Environment.CurrentDirectory, path.Substring(1));
			}

			// return the path
			return path;
		}

		/// <summary>
		/// Gets the SHA hash of the the last message inserted.
		/// </summary>
		/// <returns>The sha of the last message inserted.</returns>
		protected string GetLastMessageSha()
		{
			// Create the storage var
			string lastMessageSha = "";

			// Get the file path
			string lastMessagePath = GetLastMessageFilePath();

			if (File.Exists(lastMessagePath))
			{
				// Read the contents of the last message file.
				using (StreamReader reader = new StreamReader(lastMessagePath))
				{
					lastMessageSha = reader.ReadToEnd();
				}
			}

			// return the last message sha
			return lastMessageSha;
		}

		/// <summary>
		/// Writes the last message sha to the file.
		/// </summary>
		/// <param name="lastMessageSha">The sha has of the last message to write.</param>
		protected void WriteLastMessageSha(string lastMessageSha)
		{
			// If the last message sha is null, empty or whitespace, log and return without writing anyting
			if (String.IsNullOrWhiteSpace(lastMessageSha))
			{
				Log.Warn("The last message sha could not be written as the sha value was empty.");
				return;
			}

			// Write the sha to the file
			using (StreamWriter writer = new StreamWriter(GetLastMessageFilePath(), false))
			{
				writer.Write(lastMessageSha);
			}

		}

		#endregion
	}
}
