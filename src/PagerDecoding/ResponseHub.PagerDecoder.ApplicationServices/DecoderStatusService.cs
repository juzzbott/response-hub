using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Decoding;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Mail;
using Enivate.ResponseHub.Common.Constants;

namespace Enivate.ResponseHub.PagerDecoder.ApplicationServices
{
	public class DecoderStatusService
	{

		protected ILogger Log
		{
			get
			{
				return ServiceLocator.Get<ILogger>();
			}
		}

		protected IDecoderStatusRepository DecoderStatusRepository
		{
			get
			{
				return ServiceLocator.Get<IDecoderStatusRepository>();
			}
		}

		/// <summary>
		/// The invalid message threshold key.
		/// </summary>
		private string _invalidMessagesThresholdKey = "InvalidMessageThreshold";

		public async Task CheckInvalidMessages()
		{
			// Get the current decoder status
			DecoderStatus decoderStatus = await DecoderStatusRepository.GetDecoderStatus();

			// If there are no invalid messages, return
			if (decoderStatus.InvalidMessages.Count == 0)
			{
				return;
			}

			// If there was an email warning sent in the last 24 hours, just exit
			if (DateTime.UtcNow.AddHours(24) > decoderStatus.LastEmailWarning)
			{
				await Log.Debug("Invalid message warning email already sent in the previous 24 hours.");
				return;
			}

			// Default the threshold to 10
			int threshold = 10;
			Int32.TryParse(ConfigurationManager.AppSettings[_invalidMessagesThresholdKey], out threshold);

			// If there are at least the configured amount of invalid messages in the last hour, then send the warning email.
			int lastHourCount = decoderStatus.InvalidMessages.Count(i => i.Key >= DateTime.UtcNow.AddHours(-1));
			if (lastHourCount >= threshold)
			{
				// Send the warning email
				await SendInvalidMessageWarningEmail(lastHourCount, threshold);

				// Clear the messages from the decoder status and update the last email warning timestamp
				await DecoderStatusRepository.ClearInvalidMessages();
				await DecoderStatusRepository.SetLastEmailWarningTimestamp(DateTime.UtcNow);
			}
		}

		/// <summary>
		/// Send the email for the invalid message warning.
		/// </summary>
		/// <returns></returns>
		private async Task SendInvalidMessageWarningEmail(int invalidMessageCount, int threshold)
		{

			// Create the replacements
			IDictionary<string, string> replacements = new Dictionary<string, string>();
			replacements.Add(new KeyValuePair<string, string>("#MessageCount#", invalidMessageCount.ToString()));
			replacements.Add(new KeyValuePair<string, string>("#ThresholdAmount#", threshold.ToString()));

			// Create the mail provider and send the message
			MailProvider mailProvider = new MailProvider();
			await mailProvider.SendMailMessage(MailTemplates.PasswordChanged, replacements, null, null);

		}

	}
}
