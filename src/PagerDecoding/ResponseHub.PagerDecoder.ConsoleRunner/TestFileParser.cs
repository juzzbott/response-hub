using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Addresses.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Units.Interface;

namespace Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers
{
    public class TestFileParser : BasePagerSourceParser
    {

        public TestFileParser(ILogger log, IMapIndexRepository mapIndexRepository, IDecoderStatusRepository decoderStatusRepository, IJobMessageService jobMessageService, IAddressService addressService, ICapcodeService capcodeService)
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
            JobMessageParser = new JobMessageParser(AddressService, MapIndexRepository, Log);
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
            List<PagerMessage> pagerMessages = GetPagerMessagesFromFile();

            // Order the pager messages by timestamp descending
            pagerMessages = pagerMessages.OrderByDescending(i => i.Timestamp).ToList();

            // Loop through the pager messages
            foreach (PagerMessage pagerMessage in pagerMessages)
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

        private List<PagerMessage> GetPagerMessagesFromFile()
        {

            List<PagerMessage> pagerMessages = new List<PagerMessage>();

            // Create the pager message parser
            PagerMessageParser parser = new PagerMessageParser(Log);

            // Get the path to the test messages
            string testMessagesFilePath = String.Format("{0}\\test_pager_messages.txt", Environment.CurrentDirectory);

            // Ensure it exists
            if (!File.Exists(testMessagesFilePath))
            {
                Console.WriteLine(String.Format("Test message file path '{0}' not found.", testMessagesFilePath));
            }

            // Read the test messsages from the file
            IList<string> testMessages = new List<string>();
            using (StreamReader reader = new StreamReader(testMessagesFilePath))
            {
                while (reader.Peek() != -1)
                {
                    testMessages.Add(reader.ReadLine());
                }
            }

            // Loop through the lines, split by $$
            foreach(string testMessage in testMessages)
            {
                string[] testMessageParts = testMessage.Split(new string[] { "$$" }, StringSplitOptions.None);

                // Get the information from the element
                string capcode = testMessageParts[0];
                DateTime timestamp = DateTime.ParseExact(testMessageParts[1], "HH:mm:ss yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
                string message = testMessageParts[2];

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

            return pagerMessages;
        }
    }

}
