using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.Model.Attachments.Interface;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.UI.Models.Messages;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;
using Enivate.ResponseHub.Model.Attachments;
using Enivate.ResponseHub.UI.Helpers;
using Enivate.ResponseHub.Model.SignIn.Interface;
using Enivate.ResponseHub.Model.SignIn;

namespace Enivate.ResponseHub.UI.Controllers
{
	public abstract class BaseJobsMessagesController : BaseController
	{

		protected readonly ICapcodeService CapcodeService = ServiceLocator.Get<ICapcodeService>();
		protected readonly IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();
		protected readonly IUnitService UnitService = ServiceLocator.Get<IUnitService>();
		protected readonly ISignInEntryService SignInEntryService = ServiceLocator.Get<ISignInEntryService>();
		protected readonly IAttachmentService AttachmentService = ServiceLocator.Get<IAttachmentService>();

		#region Helpers

		protected async Task<JobMessageListViewModel> GetAllJobsMessagesViewModel(Guid userId, MessageType messageType)
		{


			// Get the capcodes for the current user
			IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(userId);

			// create the job messages list
			IList<JobMessage> jobMessages;

			int count = 50;
			Int32.TryParse(ConfigurationManager.AppSettings["JobMessages.DefaultResultLimit"], out count);
			int skip = 0;

			// Determine if filter is applied
			bool filterApplied = false;

			// If there are no job messages between dates, then just return the most recent
			if (String.IsNullOrEmpty(Request.QueryString["date_from"]) && String.IsNullOrEmpty(Request.QueryString["date_to"]))
			{
				// Get the messages for the capcodes
				jobMessages = await JobMessageService.GetMostRecent(capcodes.Select(i => i.CapcodeAddress), messageType, count, skip);
			}
			else
			{

				// Get the date from an date to values
				DateTime? dateFrom = null;
				DateTime? dateTo = null;

				// If there is a date from, set it
				if (!String.IsNullOrEmpty(Request.QueryString["date_from"]))
				{
					dateFrom = DateTime.ParseExact(Request.QueryString["date_from"], "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
					filterApplied = true;
				}

				// If there is a date from, set it
				if (!String.IsNullOrEmpty(Request.QueryString["date_to"]))
				{
					dateTo = DateTime.ParseExact(Request.QueryString["date_to"], "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
					filterApplied = true;
				}

				// Get the messages for the capcodes
				jobMessages = await JobMessageService.GetMessagesBetweenDates(capcodes.Select(i => i.CapcodeAddress), messageType, count, skip, dateFrom, dateTo);
			}

			// Create the jobs list view model.
			JobMessageListViewModel model = CreateJobMessageListModel(capcodes, jobMessages);
			model.MessageType = messageType;
			model.Filter.FilterApplied = filterApplied;

			return model;
		}

        /// <summary>
        /// Gets the view model to get all the jobs a user has interacted with.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="messageType">The message types.</param>
        /// <returns>The ViewModel of the jobs a user has interacted with.</returns>
        protected async Task<JobMessageListViewModel> GetJobsMessagesForUserViewModel(Guid userId, MessageType messageType)
        {


            // Get the capcodes for the current user
            IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(userId);

            // create the job messages list
            IList<JobMessage> jobMessages;

            int count = 50;
            Int32.TryParse(ConfigurationManager.AppSettings["JobMessages.DefaultResultLimit"], out count);
            int skip = 0;

            // Determine if filter is applied
            bool filterApplied = false;

            // Get the messages for the capcodes
            jobMessages = await JobMessageService.GetByUserId(userId, count, skip);
            
            // Create the jobs list view model.
            JobMessageListViewModel model = CreateJobMessageListModel(capcodes, jobMessages);
            model.MessageType = messageType;
            model.Filter.FilterApplied = filterApplied;

            return model;
        }

        /// <summary>
        /// Creates the JobMessageListViewModel object from the list of messages and capcodes.
        /// </summary>
        /// <param name="capcodes"></param>
        /// <param name="jobMessages"></param>
        /// <returns></returns>
        public JobMessageListViewModel CreateJobMessageListModel(IList<Capcode> capcodes, IList<JobMessage> jobMessages)
		{
			// Create the list of job message view models
			IList<JobMessageListItemViewModel> jobMessageViewModels = new List<JobMessageListItemViewModel>();
			foreach (JobMessage jobMessage in jobMessages)
            {

                // Find a capcode that matches the job and what the user has. We just need the first to match
                string capcodeString = capcodes.Select(i => i.CapcodeAddress).Intersect(jobMessage.Capcodes.Select(i => i.Capcode)).FirstOrDefault();

                // Get the capcode for the job message
                Capcode capcode = capcodes.FirstOrDefault(i => i.CapcodeAddress == capcodeString);

				// Map the view model and add to the list
				jobMessageViewModels.Add(JobMessageListItemViewModel.FromJobMessage(jobMessage, capcode, null));

			}

			// Create the model object
			JobMessageListViewModel model = new JobMessageListViewModel()
			{
				JobMessages = jobMessageViewModels
			};
			return model;
		}

		public static async Task<JobMessageViewModel> MapJobMessageToViewModel(JobMessage job, string capcodeUnitName, IList<SignInEntry> jobSignIns, IList<IdentityUser> signInUsers, Unit unit, Guid currentUserId)
		{

			IUserService userService = ServiceLocator.Get<IUserService>();

			// Map the job notes to the list of job notes view models
			IList<JobNoteViewModel> jobNotesModels = await JobMessageModelHelper.MapJobNotesToViewModel(job.Notes, userService);

			JobMessageViewModel model = new JobMessageViewModel()
			{
				Capcode = job.Capcodes.FirstOrDefault(i => i.Capcode == unit.Capcode).Capcode,
				CapcodeUnitName = capcodeUnitName,
				Id = job.Id,
				JobNumber = job.JobNumber,
				Location = job.Location,
				MessageBody = job.MessageContent,
				AdditionalMessages = job.AdditionalMessages,
				Notes = jobNotesModels,
				Priority = job.Capcodes.FirstOrDefault(i => i.Capcode == unit.Capcode).Priority,
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

			// If the unit is not null, set the lhq coordinates
			if (unit != null)
			{
				model.LhqCoordinates = unit.HeadquartersCoordinates;
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