using Enivate.ResponseHub.Model.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Events
{
	public class AssignedJobViewModel
	{

		public Guid Id { get; set; }

		public string JobNumber { get; set; }

		public string MessageBody { get; set; }

		public DateTime Timestamp { get; set; }

		/// <summary>
		/// Maps the view model from the job message.
		/// </summary>
		/// <param name="jobMessage">The job message to map.</param>
		/// <returns>The job message view model.</returns>
		public static AssignedJobViewModel FromJobMessage(JobMessage jobMessage)
		{
			// if the job message is null just return null
			if (jobMessage == null)
			{
				return null;
			}

			// Map to the assigned job view model
			return new AssignedJobViewModel()
			{
				Id = jobMessage.Id,
				JobNumber = jobMessage.JobNumber,
				MessageBody = jobMessage.MessageContent,
				Timestamp = jobMessage.Timestamp
			};

		}

	}
}