using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.UI.Models.Messages;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.SignIn;
using Enivate.ResponseHub.Model.Events.Interface;
using Enivate.ResponseHub.Model.Events;
using Enivate.ResponseHub.Model.Crews;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("jobs")]
    public class JobsController : BaseJobsMessagesController
	{

		protected readonly IEventService EventService = ServiceLocator.Get<IEventService>();

		[Route]
		public async Task<ActionResult> Index()
		{

			// Determine if the user has any active events, if there is, redirect to active event jobs, otherwise redirect to all events
			int activeEvents = await EventService.CountActiveEventsForUser(UserId);

			// If there are active events, redirect to job messages for the event, otherwise redirect to all messages
			if (activeEvents > 0)
			{
				// Build the event job list
				IList<EventJobListViewModel> model = await GetEventJobsListModel(UserId);

				// return the view
				return View("EventJobs", model);
			}
			else
			{

				// Get the initial jobs list
				JobMessageListViewModel model = await GetAllJobsMessagesViewModel(UserId, MessageType.Job);

				return View("AllJobs", model);
			}
		}

		[Route("event-jobs")]
		public async Task<ActionResult> EventJobs()
		{

			// Build the event job list
			IList<EventJobListViewModel> model = await GetEventJobsListModel(UserId);

			// return the view
			return View(model);

		}

		// GET: Jobs
		[Route("all-jobs")]
        public async Task<ActionResult> AllJobs()
		{

			// Get the initial jobs list
			JobMessageListViewModel model = await GetAllJobsMessagesViewModel(UserId, MessageType.Job);

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

				// Get the units based on the capcode
				Unit unit = await UnitService.GetUnitByCapcode(capcode);

				// Get the sign ins for the job
				IList<SignInEntry> jobSignIns = await SignInEntryService.GetSignInsForJobMessage(job.Id);

				// Get the list of users who signed in for the job
				IList<IdentityUser> signInUsers = await UserService.GetUsersByIds(jobSignIns.Select(i => i.UserId));

				// Create the model object.
				JobMessageViewModel model = await MapJobMessageToViewModel(job, capcode.ToString(), jobSignIns, signInUsers, unit, UserId);

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

		[Route("{jobId:guid}/sign-in")]
		public async Task<ActionResult> SignIn(Guid jobId)
		{

			// Get the job based on the id
			JobMessage job = await JobMessageService.GetById(jobId);

			// If the job is null, return 404
			if (job == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Get the capcode for the message
			Capcode capcode = await CapcodeService.GetByCapcodeAddress(job.Capcode);

			// Get the units based on the capcode
			Unit unit = await UnitService.GetUnitByCapcode(capcode);

			// Get the sign ins for the job
			IList<SignInEntry> jobSignIns = await SignInEntryService.GetSignInsForJobMessage(job.Id);

			// Create the model
			JobMessageSignInViewModel model = new JobMessageSignInViewModel()
			{
				JobId = jobId,
				JobNumber = job.JobNumber,
				Priority = job.Priority,
				Timestamp = job.Timestamp.ToLocalTime(),
				CapcodeUnitName = capcode.ToString(),
				SelectedMembers = String.Format("{0}|", String.Join("|", jobSignIns.Select(i => i.UserId).ToList()))
			};

			// Get the list of members for the unit
			IList<IdentityUser> unitMembers = await UnitService.GetUsersForUnit(unit.Id);

			// Loop through the users and add to the list
			foreach(IdentityUser member in unitMembers)
			{
				model.AvailableMembers.Add(member.Id, member.FullName);
			}

			// If the current user is not signed in, then set the UserToSignIn so they are selected by default
			if (!jobSignIns.Any(i => i.UserId == UserId))
			{
				model.UserToSignIn = UserId.ToString();
			}

			return View(model);
		}

		[Route("{jobId:guid}/sign-in")]
		[HttpPost]
		public async Task<ActionResult> SignIn(Guid jobId, JobMessageSignInViewModel model)
		{

			// Get the job based on the id
			JobMessage job = await JobMessageService.GetById(jobId);

			// If the job is null, return 404
			if (job == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Get the capcode for the message
			Capcode capcode = await CapcodeService.GetByCapcodeAddress(job.Capcode);

			// Get the units based on the capcode
			Unit unit = await UnitService.GetUnitByCapcode(capcode);

			// Get the sign ins for the job
			IList<SignInEntry> jobSignIns = await SignInEntryService.GetSignInsForJobMessage(job.Id);

			// Create the model
			model.JobId = jobId;
			model.JobNumber = job.JobNumber;
			model.Priority = job.Priority;
			model.Timestamp = job.Timestamp.ToLocalTime();
			model.CapcodeUnitName = capcode.ToString();

			// Get the list of members for the unit
			IList<IdentityUser> unitMembers = await UnitService.GetUsersForUnit(unit.Id);

			// Loop through the users and add to the list
			foreach (IdentityUser member in unitMembers)
			{
				model.AvailableMembers.Add(member.Id, member.FullName);
			}

			try
			{

				// Get the list of Guids that have been selected
				IList<Guid> selectedMemeberIds = model.SelectedMembers.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(i => new Guid(i)).ToList();

				// Remove users already signed in
				selectedMemeberIds = selectedMemeberIds.Except(jobSignIns.Select(i => i.UserId)).ToList();

				// Create the sign ins for the users 
				if (selectedMemeberIds.Count > 0)
				{
					await SignInEntryService.SignUsersIntoJob(jobId, job.JobNumber, DateTime.UtcNow, selectedMemeberIds, unit.Id);
				}

				// redirec the user back to the job
				return new RedirectResult(String.Format("/jobs/{0}", jobId));


			}
			catch (Exception ex)
			{
				// Log and display the error
				await Log.Error(String.Format("Error signing users into the job. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "There was a problem signing the users into the job.");
				return View(model);
			}
		}

		#region Helpers

		/// <summary>
		/// Loads the active events model list for the user.
		/// </summary>
		/// <param name="userId">The user id of the current user to get the events and jobs for.</param>
		/// <returns></returns>
		private async Task<IList<EventJobListViewModel>> GetEventJobsListModel(Guid userId)
		{
			// Get the list of active events for the user
			IList<Event> events = await EventService.GetActiveEventsForUser(userId);

			// Get the capcodes for the current user
			IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(userId);

			// Get the distinct job ids from the list of events
			IList<Guid> jobMessageIds = events.SelectMany(i => i.JobMessageIds).Distinct().ToList();

			// Gets all the job messages for the specified ids
			IList<JobMessage> allJobs = await JobMessageService.GetByIds(jobMessageIds);

			// Create the list event job list view models
			IList<EventJobListViewModel> eventJobs = new List<EventJobListViewModel>();

			// Loop through the events
			foreach(Event eventObj in events)
			{

				// Create the event job list view model
				EventJobListViewModel model = new EventJobListViewModel()
				{
					EventId = eventObj.Id,
					EventName = eventObj.Name,
					EventDescription = eventObj.Description
				};

				// Loop through the job messages for the event
				foreach(JobMessage jobMessage in allJobs.Where(i => jobMessageIds.Contains(i.Id)))
				{

					// Get the capcode for the job
					Capcode capcode = capcodes.FirstOrDefault(i => i.CapcodeAddress == jobMessage.Capcode);

					// Add the JobMessageListItemViewModel model to the list
					JobMessageListItemViewModel listItemModel = JobMessageListItemViewModel.FromJobMessage(jobMessage, capcode, null);

					// If the model was created, add to the list.
					if (listItemModel != null)
					{
						model.Jobs.Add(listItemModel);
					}

				}

				// Add the event jobs model to the list
				eventJobs.Add(model);

				// Now that we have all the jobs for the event, get the subset that are assigned to the current user
				// Find the crew the user is assigned to
				Crew userCrew = eventObj.Crews.FirstOrDefault(i => i.CrewMembers.Contains(UserId));

				// If the crew exists, get the jobs assigned to that crew
				if (userCrew != null)
				{
					model.MyJobs = model.Jobs.Where(i => userCrew.JobMessageIds.Contains(i.Id)).ToList();
				}

			}

			// return the events
			return eventJobs;

		}

		#endregion
	}
}