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


		public frmMain()
		{
			InitializeComponent();
		}

		private void frmMain_Load(object sender, EventArgs e)
		{

			// Load the configuration file
			_configuration = JsonConvert.DeserializeObject<GeneratorConfiguration>(File.ReadAllText("config.json"));

			// Instantiate the job message parser
			_jobMessageParser = new JobMessageParser(MapIndexRepository, Log);

		}

		private void btnGenerate_Click(object sender, EventArgs e)
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

				// Set the button text
				btnGenerate.Text = "Stop generating...";

				// Set the progress bar
				prgGenerating.Style = ProgressBarStyle.Marquee;
				prgGenerating.MarqueeAnimationSpeed = 30;

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

		private void generatorTimer_Tick(object sender, EventArgs e)
		{
			// Generate the messages
			GenerateMessages();
		}

		private void GenerateMessages()
		{
			
			// Get the min and max messages
			string[] minMaxMessages = ddlGenerationAmount.Items[ddlGenerationAmount.SelectedIndex].ToString().Split(new char[] { '-' });
			int minMessages = Int32.Parse(minMaxMessages[0]);
			int maxMessages = Int32.Parse(minMaxMessages[0]);

			// Generate the number of messages
			Random random = new Random(GetRandomSeed());
			int messageCount = random.Next(minMessages, (maxMessages + 1));

			// Create the pager messages dictionary
			IDictionary<string, JobMessage> jobMessages = new Dictionary<string, JobMessage>();

			// Loop through the message count
			for(int i = 0; i < messageCount; i++)
			{
				// Generate a pager message
				PagerMessage pagerMessage = GeneratePagerMessage();

				// Parse it to the Job message
				JobMessage jobMessage = _jobMessageParser.ParseMessage(pagerMessage);

				// Add to the dictionary
				jobMessages.Add(pagerMessage.ShaHash, jobMessage);
			}

			// Submit the job messages


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

