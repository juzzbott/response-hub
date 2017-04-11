using System;
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
using Enivate.ResponseHub.Model.SignIn;
using System.Globalization;

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

			// create the job messages list
			IList<JobMessage> jobMessages;

			int count = 5;
			int skip = 0;

			// Determine if filter is applied
			bool filterApplied = false;

			// If there are no job messages between dates, then just return the most recent
			if (String.IsNullOrEmpty(Request.QueryString["date_from"]) && String.IsNullOrEmpty(Request.QueryString["date_to"]))
			{
				// Get the messages for the capcodes
				jobMessages = await JobMessageService.GetMostRecent(capcodes, MessageType.Job, count, skip);
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
				jobMessages = await JobMessageService.GetMessagesBetweenDates(capcodes, MessageType.Job, count, skip, dateFrom, dateTo);
			}

			// Create the jobs list view model.
			JobMessageListViewModel model = await CreateJobMessageListModel(capcodes, jobMessages);
			model.MessageType = MessageType.Job;
			model.Filter.FilterApplied = filterApplied;

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

				DateTime cancelTime = DateTime.Now;

				// Cancel the job
				await JobMessageService.SaveProgress(id, cancelTime, UserId, MessageProgressType.Cancelled);

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

		[Route("{jobId:guid}/remove-attachment/{attachmentId:guid}")]
		public async Task<ActionResult> RemoveAttachment(Guid jobId, Guid attachmentId)
		{
			// Get the job message from the database
			JobMessage job = await JobMessageService.GetById(jobId);

			// if the job is null, throw 404
			if (job == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "Not found.");
			}

			// TODO: Ensure the user can delete this attachment

			// Remove the attachment from the job
			await JobMessageService.RemoveAttachmentFromJob(jobId, attachmentId);

			// Clear the temp attachment files
			AttachmentService.ClearAttachmentCache(attachmentId);

			// redirect back to the job
			return new RedirectResult(String.Format("/jobs/{0}?attachment_removed=1", jobId));

		}
	}
}