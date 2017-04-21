using Enivate.ResponseHub.Model.Events;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Events
{
	public class EventJobViewModel
	{

		public Guid Id { get; set; }

		public string JobNumber { get; set; }

		public string MessageBody { get; set; }

		public DateTime Timestamp { get; set; }

		public bool Assigned { get; set; }

		public EventJobStatus Status { get; set; }

		public Coordinates Coordinates { get; set; }

		/// <summary>
		/// Maps the view model from the job message.
		/// </summary>
		/// <param name="jobMessage">The job message to map.</param>
		/// <returns>The job message view model.</returns>
		public static EventJobViewModel FromJobMessage(JobMessage jobMessage)
		{
			// if the job message is null just return null
			if (jobMessage == null)
			{
				return null;
			}

			// Get the status for the event
			EventJobStatus status = EventJobStatus.NotStarted;

			if (jobMessage.ProgressUpdates.FirstOrDefault(i => i.ProgressType == MessageProgressType.Cancelled) != null)
			{
				status = EventJobStatus.Cancelled;
			}
			else if (jobMessage.ProgressUpdates.FirstOrDefault(i => i.ProgressType == MessageProgressType.JobClear) != null)
			{
				status = EventJobStatus.Completed;
			}
			else if (jobMessage.ProgressUpdates.FirstOrDefault(i => i.ProgressType == MessageProgressType.OnRoute) != null ||
				jobMessage.ProgressUpdates.FirstOrDefault(i => i.ProgressType == MessageProgressType.OnScene) != null)
			{
				status = EventJobStatus.InProgress;
			}

			// Map to the assigned job view model
			return new EventJobViewModel()
			{
				Id = jobMessage.Id,
				JobNumber = jobMessage.JobNumber,
				MessageBody = jobMessage.MessageContent,
				Timestamp = jobMessage.Timestamp,
				Coordinates = (jobMessage.Location != null ? jobMessage.Location.Coordinates : null),
				Status = status
			};

		}

	}
}