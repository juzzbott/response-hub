﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.UI.Models.Messages;
using System.Web;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.SignIn;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("messages")]
	public class MessagesController : BaseJobsMessagesController
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
			IList<JobMessage> messages = await JobMessageService.GetMostRecent(capcodes, MessageType.Message, 50);

			// Create the message list view model.
			JobMessageListViewModel model = await CreateJobMessageListModel(capcodes, messages);

			return View("~/Views/Jobs/Index.cshtml", model);
		}

		[Route("{id:guid}")]
		public async Task<ActionResult> ViewMessage(Guid id)
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

				// Get the sign ins for the job
				IList<SignInEntry> jobSignIns = await SignInEntryService.GetSignInsForJobMessage(job.Id);

				// Get the list of users who signed in for the job
				IList<IdentityUser> signInUsers = await UserService.GetUsersByIds(jobSignIns.Select(i => i.UserId));

				// Create the model object.
				JobMessageViewModel model = await MapJobMessageToViewModel(job, capcode.FormattedName(), jobSignIns, signInUsers);
				
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

	}
}