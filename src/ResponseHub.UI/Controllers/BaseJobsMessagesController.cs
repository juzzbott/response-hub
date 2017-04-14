using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Attachments.Interface;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.UI.Models.Messages;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;
using System.Threading.Tasks;
using Enivate.ResponseHub.Model.Attachments;
using System.IO;
using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.UI.Helpers;
using Enivate.ResponseHub.Model.SignIn.Interface;
using Enivate.ResponseHub.Model.SignIn;

namespace Enivate.ResponseHub.UI.Controllers
{
	public abstract class BaseJobsMessagesController : BaseController
	{

		protected readonly ICapcodeService CapcodeService = ServiceLocator.Get<ICapcodeService>();
		protected readonly IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();
		protected readonly IGroupService GroupService = ServiceLocator.Get<IGroupService>();
		protected readonly ISignInEntryService SignInEntryService = ServiceLocator.Get<ISignInEntryService>();
		protected readonly IAttachmentService AttachmentService = ServiceLocator.Get<IAttachmentService>();

		#region Helpers

		/// <summary>
		/// Creates the JobMessageListViewModel object from the list of messages and capcodes.
		/// </summary>
		/// <param name="capcodes"></param>
		/// <param name="jobMessages"></param>
		/// <returns></returns>
		public async Task<JobMessageListViewModel> CreateJobMessageListModel(IList<Capcode> capcodes, IList<JobMessage> jobMessages)
		{

			// Get all the sign ins for the messages
			IList<SignInEntry> allSignIns = await SignInEntryService.GetSignInsForJobMessages(jobMessages.Select(i => i.Id));

			// Get all the users for the job sign ins
			IList<IdentityUser> allSignInUsers = await UserService.GetUsersByIds(allSignIns.Select(i => i.UserId));

			// Create the list of job message view models
			IList<JobMessageViewModel> jobMessageViewModels = new List<JobMessageViewModel>();
			foreach (JobMessage jobMessage in jobMessages)
			{

				// Get the sign ins for the job
				IList<SignInEntry> jobSignIns = allSignIns.Where(i => i.OperationDetails.JobId == jobMessage.Id).ToList();

				// Get the list of users who signed in for the job
				IList<IdentityUser> signInUsers = allSignInUsers.Where(i => jobSignIns.Select(u => u.UserId).Contains(i.Id)).ToList();

				// Get the capcode for the job message
				Capcode capcode = capcodes.FirstOrDefault(i => i.CapcodeAddress == jobMessage.Capcode);

				// Map the view model and add to the list
				jobMessageViewModels.Add(await MapJobMessageToViewModel(jobMessage, capcode.FormattedName(), jobSignIns, signInUsers, null));

			}

			// Create the model object
			JobMessageListViewModel model = new JobMessageListViewModel()
			{
				Messages = jobMessageViewModels,
				UserCapcodes = capcodes
			};
			return model;
		}

		public static async Task<JobMessageViewModel> MapJobMessageToViewModel(JobMessage job, string capcodeGroupName, IList<SignInEntry> jobSignIns, IList<IdentityUser> signInUsers, Group group)
		{

			IUserService userService = ServiceLocator.Get<IUserService>();

			// Map the job notes to the list of job notes view models
			IList<JobNoteViewModel> jobNotesModels = await JobMessageModelHelper.MapJobNotesToViewModel(job.Notes, userService);

			JobMessageViewModel model = new JobMessageViewModel()
			{
				Capcode = job.Capcode,
				CapcodeGroupName = capcodeGroupName,
				Id = job.Id,
				JobNumber = job.JobNumber,
				Location = job.Location,
				MessageBody = job.MessageContent,
				Notes = jobNotesModels,
				Priority = job.Priority,
				Timestamp = job.Timestamp.ToLocalTime(),
				Version = job.Version
			};

			// Set the on route, on scene, job clear values
			model.OnRoute = await GetProgressModel(job, MessageProgressType.OnRoute);
			model.OnScene = await GetProgressModel(job, MessageProgressType.OnScene);
			model.JobClear = await GetProgressModel(job, MessageProgressType.JobClear);
			model.Cancelled = await GetProgressModel(job, MessageProgressType.Cancelled);

			// Get the attachments for the jobs
			IAttachmentService attachmentService = ServiceLocator.Get<IAttachmentService>();

			IList<Attachment> attachments = await attachmentService.GetAttachmentsById(job.AttachmentIds);
			IList<Attachment> imageAttachments = new List<Attachment>();

			// loop through each attachment. Any that are images, add to the image list
			foreach(Attachment attachment in attachments)
			{
				// Get the extension
				string ext = Path.GetExtension(attachment.Filename);

				// If the extension is in the list of image extensions, add the attachment to the imageAttachments list
				if (GeneralConstants.ImageExtensions.Contains(ext.ToLower()))
				{
					imageAttachments.Add(attachment);
				}
			}
			

			// Add all the attachments
			model.Attachments = attachments;
			model.ImageAttachments = imageAttachments;

			// If there are sign ins and users, then map them to the view models
			if (jobSignIns != null && jobSignIns.Count > 0 && signInUsers != null && signInUsers.Count > 0)
			{
				// Loop through the sign ins
				foreach(SignInEntry signIn in jobSignIns)
				{

					// Get the user
					IdentityUser signInUser = signInUsers.FirstOrDefault(i => i.Id == signIn.UserId);

					// Ensure the user is not null, then map to the model
					if (signInUser != null)
					{
						model.SignIns.Add(new JobMessageSignInEntry()
						{
							FullName = signInUser.FullName,
							MemberNumber = signInUser.Profile.MemberNumber,
							SignInTime = signIn.SignInTime
						});
					}

				}
			}

			// If the group is not null, set the lhq coordinates
			if (group != null)
			{
				model.LhqCoordinates = group.HeadquartersCoordinates;
			}

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