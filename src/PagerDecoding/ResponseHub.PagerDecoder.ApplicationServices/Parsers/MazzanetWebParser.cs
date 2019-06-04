using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Addresses.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using HtmlAgilityPack;
using System.Globalization;
using Enivate.ResponseHub.Model.Units.Interface;

namespace Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers
{
	public class MazzanetWebParser : BasePagerSourceParser
	{

		/// <summary>
		/// The SES URL key.
		/// </summary>
		private string _sesUrlKey = "Mazzanet.SesUrl";

		/// <summary>
		/// The CFA URL key.
		/// </summary>
		private string _cfaUrlKey = "Mazzanet.CfaUrl";

		public MazzanetWebParser(ILogger log, IMapIndexRepository mapIndexRepository, IDecoderStatusRepository decoderStatusRepository, IJobMessageService jobMessageService, IAddressService addressService, ICapcodeService capcodeService)
		{
			// Instantiate the interfaces.
			Log = log;
			MapIndexRepository = mapIndexRepository;
			DecoderStatusRepository = decoderStatusRepository;
			JobMessageService = jobMessageService;
			AddressService = addressService;
            CapcodeService = capcodeService;

			// Initialise the list of pager messages to submit.
			PagerMessagesToSubmit = new List<PagerMessage>();
			JobMessagesToSubmit = new Dictionary<string, JobMessage>();
            CapcodesToSubmit = new Dictionary<string, string>();

            // Instantiate the message parsers
            PagerMessageParser = new PagerMessageParser(Log);
			JobMessageParser = new JobMessageParser(AddressService, JobMessageService, MapIndexRepository, Log);
		}

		/// <summary>
		/// Gets the latest messages from the Mazzanet website
		/// </summary>
		public override void GetLatestMessages()
		{
			// Clear the last message list, just in case
			PagerMessagesToSubmit.Clear();
			JobMessagesToSubmit.Clear();

			// Get the last message sha
			LastInsertedMessageSha = GetLastMessageSha();

			//Create the list of pager messages
			List<PagerMessage> pagerMessages = new List<PagerMessage>();

			// Get the SES pager messages
			pagerMessages.AddRange(GetPagerMessagesFromUrl(ConfigurationManager.AppSettings[_sesUrlKey]));

			// Get the CFA pager messages
			pagerMessages.AddRange(GetPagerMessagesFromUrl(ConfigurationManager.AppSettings[_cfaUrlKey]));

			// Order the pager messages by timestamp descending
			pagerMessages = pagerMessages.OrderByDescending(i => i.Timestamp).ToList();

			// Loop through the pager messages
			foreach(PagerMessage pagerMessage in pagerMessages)
			{
				
				// If the pager sha matches the last inserted sha, then exit the loop, as no need to process any further messages.
				if (pagerMessage.ShaHash.Equals(LastInsertedMessageSha, StringComparison.CurrentCultureIgnoreCase))
				{
					Log.Info(String.Format("Last message hash detected. Exiting log file. Last message hash: '{0}'.", pagerMessage.ShaHash));
					break;
				}

				// the pager message to be inserted
				PagerMessagesToSubmit.Add(pagerMessage);
			}

			// Parse the pager messages to job messages and write to the database
			ParseAndSubmitJobMessages();

			// Write some stats to the log files.
			Log.Info(String.Format("Processed and submitted '{0}' job message{1}", JobMessagesToSubmit.Count, (JobMessagesToSubmit.Count != 1 ? "s" : "")));

            // Extract list of capcodes from the pager messages
            ExtractAndSubmitCapcodesFromPagerMessages();

            // Write some stats to the log files.
            Log.Info(String.Format("Processed and submitted '{0}' capcode{1}", CapcodesToSubmit.Count, (CapcodesToSubmit.Count != 1 ? "s" : "")));

            // Clear the lists
            PagerMessagesToSubmit.Clear();
			JobMessagesToSubmit.Clear();
            CapcodesToSubmit.Clear();

        }

		/// <summary>
		/// Calls the specific URL and returns the parser messages.
		/// </summary>
		/// <param name="url">The URL to call to get the pager messages from.</param>
		/// <returns>The list of pager messages from the url.</returns>
		public IList<PagerMessage> GetPagerMessagesFromUrl(string url)
		{
			// Create the web request and get the response
			HttpWebRequest request = WebRequest.CreateHttp(url);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			// If the response is null or not 200, then log the error
			if (response == null)
			{
				Log.Error(String.Format("Unable to get response from url '{0}'. Response is null.", url));
				return new List<PagerMessage>();
			}
			if (response.StatusCode != HttpStatusCode.OK)
			{
				Log.Error(String.Format("Invalid status response from url '{0}'. Response status code is: {1}.", url, response.StatusCode.GetEnumDescription()));
				return new List<PagerMessage>();
			}

			// Get the response data
			string responseData;

			// Get the response data
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			{
				responseData = reader.ReadToEnd();
			}

			// Now that we have the response data, need to parse it and read the pager messages
			return ParsePagerMessagesFromHtml(responseData);

		}

		/// <summary>
		/// Parses the pager messages from the raw HTML content and adds to the list of pager messages.
		/// </summary>
		/// <param name="responseData">The HTML response data from the website.</param>
		/// <returns>The list of pager messages parsed from the HTML content</returns>
		public IList<PagerMessage> ParsePagerMessagesFromHtml(string responseData)
		{

			// Create the pager message parser
			PagerMessageParser parser = new PagerMessageParser(Log);

			// Create the pager messages list
			IList<PagerMessage> pagerMessages = new List<PagerMessage>();

			// Load the xml document
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(responseData);

			// Loop through each table row in the mark up
			foreach(HtmlNode node in document.DocumentNode.SelectNodes("html/body/table/tr"))
			{

				// Get the information from the element
				string capcode = node.ChildNodes[0].InnerText;
				DateTime timestamp = DateTime.ParseExact(node.ChildNodes[1].InnerText,"HH:mm:ss yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
				string message = node.ChildNodes[2].InnerText;

				// Because we only have a few details, we need to backfill to get a "PDW style" pager message
				string rawMessage = String.Format("{0} {1} POCSAG-1  ALPHA   512  {2}", capcode, timestamp.ToString("HH:mm:ss dd-MM-yy"), message);

				try
				{

					// If we should skip the message, then do so here.
					if (ShouldSkipMessage(message))
					{
						Log.Debug(String.Format("Skipping internal system message: {0}", message));
						continue;
					}

					// Parse the pager message
					PagerMessage pagerMessage = parser.ParsePagerMessage(rawMessage);

					// If the pager message is null, parsing failed, so continue to next
					if (pagerMessage == null)
					{
						continue;
					}

					// Add the pager message to the list
					pagerMessages.Add(pagerMessage);
					
				}
				catch (Exception ex)
				{
					Log.Error(String.Format("Unable to parse pager message. Raw message data: {0}", rawMessage), ex);
				}

			}

			// return the list of pager messages
			return pagerMessages;

		}
	}

}
