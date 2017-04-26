using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Models.Messages
{
	public class JobMessageListItemViewModel
	{

		public Guid Id { get; set; }

		public string MessageBody { get; set; }

		public string MessageBodyTruncated
		{
			get
			{
				return MessageBody.Truncate(60, "...");
			}
		}

		public DateTime Timestamp { get; set; }

		public string JobNumber { get; set; }

		public string Capcode { get; set; }

		public string CapcodeUnitName { get; set; }

		public MessagePriority Priority { get; set; }

		public MessageProgressViewModel OnRoute { get; set; }

		public MessageProgressViewModel OnScene { get; set; }

		public MessageProgressViewModel JobClear { get; set; }

		public MessageProgressViewModel Cancelled { get; set; }

		/// <summary>
		/// Gets the job message list item view model from the original job mesage model object.
		/// </summary>
		/// <param name="jobMessage">The job messages </param>
		/// <param name="capcode">The capcode for the job.</param>
		/// <returns>The mapped job message list item view mode.</returns>
		public static JobMessageListItemViewModel FromJobMessage(JobMessage jobMessage, Capcode capcode, IList<IdentityUser> users)
		{

			// If the job message is null, just return null
			if (jobMessage == null || capcode == null)
			{
				return null;
			}

			// build the job message list item view model
			JobMessageListItemViewModel model = new JobMessageListItemViewModel()
			{
				Capcode = capcode.CapcodeAddress,
				CapcodeUnitName = capcode.ToString(),
				Id = jobMessage.Id,
				JobNumber = jobMessage.JobNumber,
				MessageBody = jobMessage.MessageContent,
				Priority = jobMessage.Priority,
				Timestamp = jobMessage.Timestamp.ToLocalTime()
			};

			// Set the on route, on scene, job clear values
			model.OnRoute = GetProgressModel(jobMessage, MessageProgressType.OnRoute, users);
			model.OnScene = GetProgressModel(jobMessage, MessageProgressType.OnScene, users);
			model.JobClear = GetProgressModel(jobMessage, MessageProgressType.JobClear, users);
			model.Cancelled = GetProgressModel(jobMessage, MessageProgressType.Cancelled, users);

			return model;
		}

		/// <summary>
		/// Gets the progress model for the specific progress type, if it exists. 
		/// </summary>
		/// <param name="job">The job to get the progress from. </param>
		/// <param name="progressType">The progress type to get.</param>
		/// <returns></returns>
		private static MessageProgressViewModel GetProgressModel(JobMessage job, MessageProgressType progressType, IList<IdentityUser> users)
		{
			MessageProgress progress = job.ProgressUpdates.FirstOrDefault(i => i.ProgressType == progressType);
			if (progress != null)
			{
				MessageProgressViewModel progressModel = new MessageProgressViewModel()
				{
					Timestamp = progress.Timestamp.ToLocalTime(),
					UserId = progress.UserId,
					ProgressType = progress.ProgressType
				};

				// if users are not null, then load the user details from the list if it exists
				if (users != null)
				{
					// Get the user
					IdentityUser user = users.FirstOrDefault(i => i.Id == progress.UserId);

					// If it's not null, then set the user details
					if (user != null)
					{
						progressModel.UserFullName = user.FullName;
					}
				}

				return progressModel;
			}
			else
			{
				return null;
			}
		}

	}
}