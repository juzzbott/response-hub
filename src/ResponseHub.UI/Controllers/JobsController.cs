﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

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

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("jobs")]
    public class JobsController : Controller
	{

		private ILogger _log;
		protected ILogger Log
		{
			get
			{
				return _log ?? (_log = UnityConfiguration.Container.Resolve<ILogger>());
			}
		}

		private ICapcodeService _capcodeService;
		protected ICapcodeService CapcodeService
		{
			get
			{
				return _capcodeService ?? (_capcodeService = UnityConfiguration.Container.Resolve<ICapcodeService>());
			}
		}

		private IJobMessageService _jobMessageService;
		protected IJobMessageService JobMessageService
		{
			get
			{
				return _jobMessageService ?? (_jobMessageService = UnityConfiguration.Container.Resolve<IJobMessageService>());
			}
		}

		// GET: Jobs
		[Route]
        public async Task<ActionResult> Index()
        {

			// Get the current user id
			Guid userId = new Guid(User.Identity.GetUserId());

			// Get the capcodes for the current user
			IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(userId);

			// Get the messages for the capcodes
			IList<JobMessage> jobMessages = await JobMessageService.GetMostRecent(capcodes.Select(i => i.CapcodeAddress), 50, MessageType.Job);

			// Create the list of job message view models
			IList<JobMessageViewModel> jobMessageViewModels = new List<JobMessageViewModel>();
			foreach(JobMessage jobMessage in jobMessages)
			{

				// Get the capcode for the job message
				Capcode capcode = capcodes.FirstOrDefault(i => i.CapcodeAddress == jobMessage.Capcode);

				// Map the view model and add to the list
				jobMessageViewModels.Add(MapJobMessageToViewModel(jobMessage, capcode.FormattedName()));

			}

			// Create the model object
			JobMessageListViewModel model = new JobMessageListViewModel()
			{
				Messages = jobMessageViewModels,
				UserCapcodes = capcodes
			};

			return View(model);
        }

		[Route("{id:guid}")]
		public async Task<ActionResult> ViewJob(Guid id)
		{

			try
			{

				// Get the job message from the database
				JobMessage job = await JobMessageService.GetById(id);

				// If the job is null, return 404
				if (job == null)
				{
					throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
				}

				// Get the capcode for the message
				Capcode capcode = await CapcodeService.GetByCapcodeAddress(job.Capcode);

				// Create the model object.
				JobMessageViewModel model = MapJobMessageToViewModel(job, capcode.FormattedName());

				// Set the progress updates.
				model.OnRoute = await GetProgressModel(job, MessageProgressType.OnRoute);
				model.OnScene = await GetProgressModel(job, MessageProgressType.OnScene);
				model.JobClear = await GetProgressModel(job, MessageProgressType.JobClear);

				// return the job view
				return View(model);

			}
			catch (Exception ex)
			{
				// Log the error
				await _log.Error(String.Format("Unable to load the job message details. Message: {0}", ex.Message, ex));
				return View(new object());

			}

		}

		#region Helpers

		private static JobMessageViewModel MapJobMessageToViewModel(JobMessage job, string capcodeGroupName)
		{
			return new JobMessageViewModel()
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
		}

		/// <summary>
		/// Gets the progress model for the specific progress type, if it exists. 
		/// </summary>
		/// <param name="job">The job to get the progress from. </param>
		/// <param name="progressType">The progress type to get.</param>
		/// <returns></returns>
		private async Task<MessageProgressViewModel> GetProgressModel(JobMessage job, MessageProgressType progressType)
		{
			MessageProgress progress = job.ProgressUpdates.FirstOrDefault(i => i.ProgressType == progressType);
			if (progress != null)
			{
				MessageProgressViewModel progressModel = new MessageProgressViewModel()
				{
					Timestamp = progress.Timestamp.ToLocalTime(),
					UserId = progress.UserId
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