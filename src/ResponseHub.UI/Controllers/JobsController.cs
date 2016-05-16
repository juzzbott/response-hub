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
    public class JobsController : BaseJobsMessagesController
	{

		

		// GET: Jobs
		[Route]
        public async Task<ActionResult> Index()
		{

			// Get the current user id
			Guid userId = new Guid(User.Identity.GetUserId());

			// Get the capcodes for the current user
			IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(userId);

			// Get the messages for the capcodes
			IList<JobMessage> jobMessages = await JobMessageService.GetMostRecent(capcodes, MessageType.Job, 50);

			// Create the jobs list view model.
			JobMessageListViewModel model = await CreateJobMessageListModel(capcodes, jobMessages);

			return View(model);
		}

		[Route("{id:guid}")]
		public async Task<ActionResult> ViewJob(Guid id)
		{

			
			// Get the job message from the database
			JobMessage job = await JobMessageService.GetById(id);

			
			// If the job is null, return 404
			if (job == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			try
			{ 

				// Get the capcode for the message
				Capcode capcode = await CapcodeService.GetByCapcodeAddress(job.Capcode);

				// Create the model object.
				JobMessageViewModel model = await MapJobMessageToViewModel(job, capcode.FormattedName());

				// return the job view
				return View(model);

			}
			catch (Exception ex)
			{
				// Log the error
				await Log.Error(String.Format("Unable to load the job details. Message: {0}", ex.Message, ex));
				return View(new object());

			}

		}

		[Route("{id:guid}/cancel-job")]
		public async Task<ActionResult> CancelJob(Guid id)
		{
			
			// Get the job message from the database
			JobMessage job = await JobMessageService.GetById(id);

			// If the job is null, return 404
			if (job == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			try
			{

				// Cancel the job
				await JobMessageService.AddProgress(id, UserId, MessageProgressType.Cancelled);

				// Redirect back to the job.
				return new RedirectResult(String.Format("/jobs/{0}", id));

			}
			catch (Exception ex)
			{
				// Log the error
				await Log.Error(String.Format("Unable to cancel the job. Message: {0}", ex.Message, ex));
				return View(new object());

			}
		}
	}
}