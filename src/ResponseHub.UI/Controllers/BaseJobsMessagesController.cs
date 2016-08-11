using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.AspNet.Identity;
using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.UI.Models.Messages;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.UI.Controllers
{
	public abstract class BaseJobsMessagesController : BaseController
	{

		protected readonly ICapcodeService CapcodeService = ServiceLocator.Get<ICapcodeService>();
		protected readonly IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();

		#region Helpers

		/// <summary>
		/// Creates the JobMessageListViewModel object from the list of messages and capcodes.
		/// </summary>
		/// <param name="capcodes"></param>
		/// <param name="jobMessages"></param>
		/// <returns></returns>
		public static async Task<JobMessageListViewModel> CreateJobMessageListModel(IList<Capcode> capcodes, IList<JobMessage> jobMessages)
		{
			// Create the list of job message view models
			IList<JobMessageViewModel> jobMessageViewModels = new List<JobMessageViewModel>();
			foreach (JobMessage jobMessage in jobMessages)
			{

				// Get the capcode for the job message
				Capcode capcode = capcodes.FirstOrDefault(i => i.CapcodeAddress == jobMessage.Capcode);

				// Map the view model and add to the list
				jobMessageViewModels.Add(await MapJobMessageToViewModel(jobMessage, capcode.FormattedName()));

			}

			// Create the model object
			JobMessageListViewModel model = new JobMessageListViewModel()
			{
				Messages = jobMessageViewModels,
				UserCapcodes = capcodes
			};
			return model;
		}

		public static async Task<JobMessageViewModel> MapJobMessageToViewModel(JobMessage job, string capcodeGroupName)
		{
			JobMessageViewModel model = new JobMessageViewModel()
			{
				Capcode = job.Capcode,
				CapcodeGroupName = capcodeGroupName,
				Id = job.Id,
				JobNumber = job.JobNumber,
				Location = job.Location,
				MessageBody = job.MessageContent,
				Notes = job.Notes,
				Priority = job.Priority,
				Timestamp = job.Timestamp.ToLocalTime()
			};

			// Set the on route, on scene, job clear values
			model.OnRoute = await GetProgressModel(job, MessageProgressType.OnRoute);
			model.OnScene = await GetProgressModel(job, MessageProgressType.OnScene);
			model.JobClear = await GetProgressModel(job, MessageProgressType.JobClear);
			model.Cancelled = await GetProgressModel(job, MessageProgressType.Cancelled);

			// return the mapped job view model
			return model;

		}
		
		/// <summary>
		/// Gets the progress model for the specific progress type, if it exists. 
		/// </summary>
		/// <param name="job">The job to get the progress from. </param>
		/// <param name="progressType">The progress type to get.</param>
		/// <returns></returns>
		public static async Task<MessageProgressViewModel> GetProgressModel(JobMessage job, MessageProgressType progressType)
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

				// Get the user who updated the progress.
				IdentityUser progressUser = await ServiceLocator.Get<IUserService>().FindByIdAsync(progress.UserId);
				if (progressUser != null)
				{
					progressModel.UserFullName = progressUser.FullName;
				}

				return progressModel;
			}
			else
			{
				return null;
			}
		}

		#endregion

	}
}