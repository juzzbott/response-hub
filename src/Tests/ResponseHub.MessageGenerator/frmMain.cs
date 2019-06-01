using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers;
using Enivate.ResponseHub.MessageGenerator.Configuration;
using System.IO;
using Enivate.ResponseHub.PagerDecoder.ApplicationServices;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.DataAccess.MongoDB;
using Enivate.ResponseHub.ApplicationServices;
using Enivate.ResponseHub.Model.Addresses.Interface;

namespace Enivate.ResponseHub.MessageGenerator
{
	public partial class frmMain : Form
	{

		/// <summary>
		/// Flag to determine if we are currently generating messages
		/// </summary>
		private bool _generating;

		/// <summary>
		/// The job message parser used to parse the generated pager messages into job messages
		/// </summary>
		private JobMessageParser _jobMessageParser;

		/// <summary>
		/// The configuration details for the generator.
		/// </summary>
		private GeneratorConfiguration _configuration;

		/// <summary>
		/// The array containing the map reference letters
		/// </summary>
		private string[] _mapReferenceLetters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };

		string _htmlDisplayHeader;
		string _htmlDisplayFooter;

		StringBuilder _htmlContent;

		protected ILogger Log
		{
			get
			{
				return ServiceLocator.Get<ILogger>();
			}
		}

		protected IMapIndexRepository MapIndexRepository
		{
			get
			{
				return ServiceLocator.Get<IMapIndexRepository>();
			}
		}

		protected IAddressService AddressService
		{
			get
			{
				return ServiceLocator.Get<IAddressService>();
			}
		}


		public frmMain()
		{
			InitializeComponent();
		}

		private void frmMain_Load(object sender, EventArgs e)
		{

			// Load the configuration file
			_configuration = JsonConvert.DeserializeObject<GeneratorConfiguration>(File.ReadAllText("config.json"));

			// Instantiate the job message parser
			_jobMessageParser = new JobMessageParser(AddressService, MapIndexRepository, Log);

			// Generate the html header
			_htmlDisplayHeader = "<html><head><style type=\"text/css\">body { font-size: 13px; font-family: Arial, sans-serif; } ul.message-list { margin-bottom: 20px; margin-top: 0; padding-left: 0px; } li.emerg { color: #C7201D; } li.non-emerg { color: #C7A216; } p {margin-bottom: 5px; }</style></head><body>";

			// generate the html footer
			_htmlDisplayFooter = "</body></html>";

			// Instantiate the HTML string builder container
			_htmlContent = new StringBuilder();

			// Set default message count option
			ddlGenerationAmount.SelectedIndex = 0;

			// Set the tooltips
			toolTip.SetToolTip(lblCapcodeToolTip, "The capcode address for the messages to be generated.");
			toolTip.SetToolTip(lblGenerationAmountToolTip, "The random amount of messages to be generated within 30 seconds.");
			toolTip.SetToolTip(lblMapPagesToolTip, "The map pages to generate the grid references in the messages for. Comma separate multiple pages.");
			toolTip.SetToolTip(lblSubmitToolTip, "Determines if the generated messages should be submitted to the test ResponseHub website.");

		}

		private async void btnGenerate_Click(object sender, EventArgs e)
		{
			
			if (!_generating)
			{

				// Start generating
				_generating = true;

				// Set the button properties
				txtCapcode.Enabled = false;
				txtMapPages.Enabled = false;
				ddlGenerationAmount.Enabled = false;
				chkEmergency.Enabled = false;
				chkNonEmergency.Enabled = false;
				chkSubmitToResponseHub.Enabled = false;

				// Set the button text
				btnGenerate.Text = "Stop generating...";

				// Set the progress bar
				prgGenerating.Style = ProgressBarStyle.Marquee;
				prgGenerating.MarqueeAnimationSpeed = 30;


				// Generate the messages
				await GenerateMessages();

				// Start the timer
				generatorTimer.Enabled = true;
				generatorTimer.Start();

			}
			else
			{

				// Set the button properties
				txtCapcode.Enabled = true;
				txtMapPages.Enabled = true;
				ddlGenerationAmount.Enabled = true;
				chkEmergency.Enabled = true;
				chkNonEmergency.Enabled = true;
				chkSubmitToResponseHub.Enabled = true;

				// Set the button text
				btnGenerate.Text = "Generate";

				// Set the progress bar;
				prgGenerating.Style = ProgressBarStyle.Blocks;
				prgGenerating.MarqueeAnimationSpeed = 0;

				// Start the timer
				generatorTimer.Enabled = false;
				generatorTimer.Stop();

				// stop generating
				_generating = false;

			}

		}

		private async void generatorTimer_Tick(object sender, EventArgs e)
		{
			// Generate the messages
			await GenerateMessages();
		}

		private async Task GenerateMessages()
		{
			
			// Get the min and max messages
			string[] minMaxMessages = ddlGenerationAmount.Items[ddlGenerationAmount.SelectedIndex].ToString().Split(new char[] { '-' });
			int minMessages = Int32.Parse(minMaxMessages[0]);
			int maxMessages = Int32.Parse(minMaxMessages[1]);

			// Generate the number of messages
			Random random = new Random(GetRandomSeed());
			int messageCount = random.Next(minMessages, (maxMessages + 1));

			// Create the pager messages dictionary
			Dictionary<string, JobMessage> jobMessages = new Dictionary<string, JobMessage>();

			// Loop through the message count
			for(int i = 0; i < messageCount; i++)
			{
				// Generate a pager message
				PagerMessage pagerMessage = GeneratePagerMessage();

				// Parse it to the Job message
				JobMessage jobMessage = await _jobMessageParser.ParseMessage(pagerMessage);

				// Add to the dictionary
				jobMessages.Add(pagerMessage.ShaHash, jobMessage);
			}

			// Submit the job messages, if the option to submit is checked.
			if (chkSubmitToResponseHub.Checked)
			{
				SubmitJobMessages(jobMessages);
			}

			// Display the generated messages
			DisplayGeneratedMessages(jobMessages);


		}

		private void SubmitJobMessages(Dictionary<string, JobMessage> jobMessages)
		{
			try
			{
				ILogger log = new FileLogger();
				IJobMessageRepository jobMessageRepository = new JobMessageRepository();
                ISignInEntryRepository signInRepository = new SignInEntryRepository();
                IAttachmentRepository attachmentRepository = new AttachmentRepository();
                IJobMessageService jobMessageService = new JobMessageService(jobMessageRepository, signInRepository, attachmentRepository, log);

				// Submit the job messages to the database
				jobMessageService.AddMessages(jobMessages.Select(i => i.Value).ToList());
			}
			catch (Exception ex)
			{
				// Show the messagebox error message
				MessageBox.Show(ex.ToString(), "Error submitting job messages", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void DisplayGeneratedMessages(IDictionary<string, JobMessage> jobMessages)
		{

			// Create the string builder to store the generated message markup
			StringBuilder generatedMessagesMarkup = new StringBuilder();

			// Add the summary
			generatedMessagesMarkup.AppendLine(String.Format("<p><strong>{0} messages generated: {1} emergency, {2} non-emergency</strong></p>",
				jobMessages.Count,
				jobMessages.Count(i => i.Value.Capcodes.First().Priority == MessagePriority.Emergency),
				jobMessages.Count(i => i.Value.Capcodes.First().Priority == MessagePriority.NonEmergency)));

			// Create the list of messages
			generatedMessagesMarkup.AppendLine("<ul class=\"message-list\">");

			// Loop through each of the job messages
			foreach (JobMessage message in jobMessages.Select(i => i.Value))
			{
				generatedMessagesMarkup.AppendLine(String.Format("<li class=\"{0}\">{1}</li>", (message.Capcodes.First().Priority == MessagePriority.Emergency ? "emerg" : "non-emerg"), message.MessageContent));
			}

			// Close the list
			generatedMessagesMarkup.AppendLine("</ul>");

			// Add the generated markup to the html content string builder
			_htmlContent.Insert(0, generatedMessagesMarkup.ToString());

			// Set the web browser content
			brsMessages.DocumentText = "0";
			brsMessages.Document.OpenNew(true);
			brsMessages.Document.Write(String.Format("{0}{1}{2}", _htmlDisplayHeader, _htmlContent.ToString(), _htmlDisplayFooter));
			brsMessages.Refresh();

		}

		/// <summary>
		/// Generates a random pager message.
		/// </summary>
		/// <returns></returns>
		private PagerMessage GeneratePagerMessage()
		{

			// Get the capcode
			string capcode = txtCapcode.Text;

			// Get the message priority
			MessagePriority priority = GetRandomPriority();

			// Generate the job number based on the priority
			// We use a random number 5 digits long after the year and date to make up the remainder of the job number
			Random random = new Random(GetRandomSeed());
			string jobNumber = String.Format("{0}{1}{2}",
				(priority == MessagePriority.Emergency ? String.Format("{0}ALERT F", JobMessageParser.EmergencyPrefix) : String.Format("{0}S", JobMessageParser.NonEmergencyPrefix)),
				DateTime.Now.ToString("yyMM"),
				random.Next(10000, 99999));

			// Get the map reference
			string mapReference = GetRandomMapReference();

			// Get the random index for the message template
			int messageTemplateIndex = random.Next(0, _configuration.MessageTemplates.Count);

			// Create the message content
			string message = String.Format(_configuration.MessageTemplates[messageTemplateIndex], jobNumber, mapReference);

			DateTime timestamp = DateTime.Now;

			// Create the pager message
			return new PagerMessage()
			{
				Address = capcode,
				Bitrate = 512,
				Id = Guid.NewGuid(),
				MessageContent = message,
				Mode = "POCSAG-1",
				ShaHash = PagerMessage.GenerateHash(capcode, timestamp, message),
				Timestamp = timestamp,
				Type = "ALPHA"
			};
		}

		/// <summary>
		/// Generates a random map reference based on the map pages selected.
		/// </summary>
		/// <returns></returns>
		private string GetRandomMapReference()
		{

			// Get the map pages
			IList<string> mapPages = GetMapPages();

			// If there are no map pages, return empty string
			if (mapPages == null || mapPages.Count == 0)
			{
				return "";
			}

			// Generate the random number
			Random random = new Random(GetRandomSeed());

			// Generate the random index for the map page, letter and number grid references
			int randomMapPageIndex = random.Next(0, mapPages.Count);
			int randomGridLetterIndex = random.Next(0, _mapReferenceLetters.Length);
			int randomGridNumber = random.Next(0, 11);

			// Get the random page
			string page = mapPages[randomMapPageIndex];

			// return the random map reference
			return String.Format("{0} {1} {2}{3}",
				(page.Length == 4 ? "SVVB C" : "M"),
				page,
				_mapReferenceLetters[randomGridLetterIndex],
				randomGridNumber
				);


		}

		/// <summary>
		/// Gets a random message priority based on the checkbox options
		/// </summary>
		/// <returns></returns>
		private MessagePriority GetRandomPriority()
		{
			// If the emergency option is not checked, then we can only have non emergency priority
			if (!chkEmergency.Checked)
			{
				return MessagePriority.NonEmergency;
			}
			else if (!chkNonEmergency.Checked)
			{
				// Non emergency isn't checked, so we can only have emergency
				return MessagePriority.Emergency;
			}
			else
			{
				// Generate random value between 1 and 2 inclusive.
				// Need to set max value to 3 as Next max value is exclusive
				Random random = new Random(GetRandomSeed());
				return (MessagePriority)random.Next(1, 3);
			}
		}

		/// <summary>
		/// Gets the map pages from the list defined in the text box.
		/// </summary>
		/// <returns></returns>
		private IList<string> GetMapPages()
		{
			// If the map pages is null or empty, return empty string
			if (String.IsNullOrEmpty(txtMapPages.Text))
			{
				return new List<string>();
			}

			// Create the list of map pages
			IList<string> mapPages = new List<string>();

			// Split the pages by comma
			foreach(string mapPage in txtMapPages.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				mapPages.Add(mapPage.Trim());
			}

			// return the map pages
			return mapPages;
		}

		/// <summary>
		/// Generates a crypto unique integer used for random seed.
		/// </summary>
		/// <returns></returns>
		private int GetRandomSeed()
		{
			// Use the RNGCrypto provider to generate a random seed value
			using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
			{
				byte[] randomBytes = new byte[4];
				provider.GetBytes(randomBytes);
				return BitConverter.ToInt32(randomBytes, 0);
			}
		}
	}
}

