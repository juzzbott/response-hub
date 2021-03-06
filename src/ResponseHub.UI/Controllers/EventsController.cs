using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Events.Interface;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.UI.Models.Events;
using Enivate.ResponseHub.Model.Events;
using System.Globalization;
using System.Net;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Agencies.Interface;
using Enivate.ResponseHub.Model.Agencies;
using Enivate.ResponseHub.Model.Crews;
using Enivate.ResponseHub.UI.Models.Messages;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.UI.Models.Users;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("events")]
	public class EventsController : BaseController
    {

		IUnitService UnitService = ServiceLocator.Get<IUnitService>();
		IEventService EventService = ServiceLocator.Get<IEventService>();
		IAgencyService AgencyService = ServiceLocator.Get<IAgencyService>();
		ICapcodeService CapcodeService = ServiceLocator.Get<ICapcodeService>();
		IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();

		// GET: Events
		[Route]
        public async Task<ActionResult> Index()
        {

			// Get the units for the user
			IList<Unit> usersUnits = await UnitService.GetUnitsForUser(UserId);

			List<Event> events = new List<Event>();

			// If there is no search term, return all results, otherwise return only those that match the search results.
			if (String.IsNullOrEmpty(Request.QueryString["q"]))
			{
				// Get the most recent units
				events.AddRange(await EventService.GetEventsByUnit(usersUnits.Select(i => i.Id).FirstOrDefault()));
			}
			else
			{
				events.AddRange(await EventService.FindByKeywords(Request.QueryString["q"], usersUnits.Select(i => i.Id).FirstOrDefault()));
			}

			// Create the list of view model items
			IList<EventListItemViewModel> model = new List<EventListItemViewModel>();
			foreach(Event eventObj in events)
			{
				// Get the unit
				Unit eventUnit = usersUnits.First(i => i.Id == eventObj.UnitId);

				// Add the list item to the model list
				model.Add(new EventListItemViewModel() {
					Id = eventObj.Id,
					Name = eventObj.Name,
					UnitId = eventUnit.Id,
					UnitName = eventUnit.Name,
					StartDate = eventObj.StartDate.ToLocalTime(),
					FinishDate = eventObj.FinishedDate,
					JobsCount = eventObj.JobMessageIds.Count,
					Description = eventObj.Description,
					CrewsCount = eventObj.Crews.Count
				});
			}

			return View(model);
        }

		#region Create Event

		[Route("create")]
		public async Task<ActionResult> Create()
		{

			// Create the model
			CreateEventViewModel model = new CreateEventViewModel();

			// Set the available units
			model.AvailableUnits = await GetAvailableUnits(UserId);

			if (model.AvailableUnits.Count == 2) // First is please select
			{
				model.UnitId = new Guid(model.AvailableUnits[1].Value);
			}

			model.Name = DateTime.Now.ToString("d MMMM yyyy");
			model.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
			model.StartTime = DateTime.Now.AddMinutes(-90).ToString("HH:mm");

			return View(model);
		}

		[Route("create")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create(CreateEventViewModel model)
		{
			// Set the available units
			model.AvailableUnits = await GetAvailableUnits(UserId);

			// If the model state is invalid, return the view with the model
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try {

				// Parse the start date
				string startDateComplete = String.Format("{0} {1}:00", model.StartDate, model.StartTime);
				DateTime startDate = DateTime.ParseExact(startDateComplete, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).ToUniversalTime();

				// Create the new event
				Event newEvent = await EventService.CreateEvent(model.Name, model.Description, model.UnitId, UserId, startDate);

				// Get the capcodes for the specified unit
				IList<Capcode> unitCapcodes = await CapcodeService.GetCapcodesForUnit(model.UnitId);

				// Get the job message ids within the start date and an hour in the future
				IList<Guid> jobMessageIds = await JobMessageService.GetMessageIdsBetweenDates(unitCapcodes.Select(i => i.CapcodeAddress), MessageType.Job, startDate, DateTime.UtcNow.AddHours(1));

				// Now that we have the job message ids, we need to set them to the event
				await EventService.SetJobsToEvent(newEvent.Id, jobMessageIds);

				// Redirect to the event
				return new RedirectResult(String.Format("/events/{0}", newEvent.Id));

			}
			catch (Exception ex)
			{
				await ServiceLocator.Get<ILogger>().Error(String.Format("Unable to create new event. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "Sorry, there was a system error creating the new event.");
				return View(model);
			}
		}

		#endregion

		#region Edit Event

		[Route("{id:guid}/edit")]
		public async Task<ActionResult> Edit(Guid id)
		{

			// Get the event based on the id
			Event eventObj = await EventService.GetById(id);

			// If the job is null, return 404
			if (eventObj == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Create the model
			EditEventViewModel model = new EditEventViewModel();

			// Set the available units
			model.Name = eventObj.Name;
			model.Description = eventObj.Description;
			model.StartDate = eventObj.StartDate.ToLocalTime().ToString("yyyy-MM-dd");
			model.StartTime = eventObj.StartDate.ToLocalTime().ToString("HH:mm");

			return View(model);
		}

		[Route("{id:guid}/edit")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit(Guid id, CreateEventViewModel model)
		{

			// Get the event based on the id
			Event eventObj = await EventService.GetById(id);

			// If the job is null, return 404
			if (eventObj == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// If the model state is invalid, return the view with the model
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{

				// Parse the start date
				string startDateComplete = String.Format("{0} {1}:00", model.StartDate, model.StartTime);
				DateTime startDate = DateTime.ParseExact(startDateComplete, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).ToUniversalTime();

				// Try to save the event
				await EventService.SaveEvent(id, model.Name, model.Description, startDate);

				// Redirect to the event
				return new RedirectResult(String.Format("/events/{0}?updated=1", id));

			}
			catch (Exception ex)
			{
				await ServiceLocator.Get<ILogger>().Error(String.Format("Unable to save event. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "Sorry, there was a system error saving the event.");
				return View(model);
			}
		}

		#endregion

		#region View event

		[Route("{id:guid}")]
		public async Task<ActionResult> ViewEvent(Guid id)
		{
			// Get the event based on the id
			Event eventObj = await EventService.GetById(id);

			// If the job is null, return 404
			if (eventObj == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Get the unit If it's null, throw system error
			Unit unit = await UnitService.GetById(eventObj.UnitId);
			if (unit == null)
			{
				throw new HttpException((int)HttpStatusCode.InternalServerError, "The unit details count not be found.");
			}

			// Create the model
			EventViewModel model = new EventViewModel()
			{
				Id = id,
				FinishedDate = eventObj.FinishedDate,
				StartDate = eventObj.StartDate,
				UnitId = eventObj.UnitId,
				UnitName = unit.Name,
				Name = eventObj.Name,
				Finished = eventObj.FinishedDate.HasValue,
				Description = eventObj.Description
			};

			TimeSpan duration;

			// If finsihed, set the duration string
			if (model.Finished)
			{
				duration = model.FinishedDate.Value - model.StartDate;
			}
			else
			{
				// Use UtcNow as StartDate is stored in UTC time.
				duration = DateTime.UtcNow - model.StartDate;
			}

			// Set the duration
			model.DurationString = String.Format("{0} days {1} hours", duration.ToString("%d"), duration.ToString("%h"));
			
			// Load the users for the model
			IList<IdentityUser> users = await UnitService.GetUsersForUnit(eventObj.UnitId);
			foreach (IdentityUser user in users)
			{
				model.AvailableMembers.Add(new Tuple<Guid, string, string>(user.Id, user.FullName, user.Profile.MemberNumber));
			}
			
			// Get the current crews
			model.Crews = await GetCrewsModelForEvent(eventObj, model.AvailableMembers);

			// Get the jobs for the event
			model.Jobs = await GetJobsForEvent(eventObj.JobMessageIds, eventObj);
			model.UnassignedJobs = model.Jobs.Where(i => !i.Assigned).OrderByDescending(i => i.JobNumber, new JobNumberComparer()).ToList();
			model.UnassignedJobsCount = model.Jobs.Count(i => !i.Assigned);
			model.InProgressJobsCount = model.Jobs.Count(i => i.Status == EventJobStatus.InProgress);
			model.CompletedJobsCount = model.Jobs.Count(i => i.Status == EventJobStatus.Completed);

			// Determine if all the available members have been assigned to crews
			if (model.AvailableMembers.Count == model.Crews.Sum(i => i.CrewMemberCount))
			{
				model.AllMembersAllocated = true;
			}

			// return the view
			return View(model);
		}

		#endregion

		#region Finish Event

		[Route("{id:guid}/finish-event")]
		public ActionResult FinishEvent(Guid id)
		{
			// Create the model and return the view
			FinishEventViewModel model = new FinishEventViewModel()
			{
				EventId = id,
				FinishDate = DateTime.Now.ToString("yyyy-MM-dd"),
				FinishTime = DateTime.Now.ToString("HH:mm")
			};

			// return the view
			return View(model);
		}

		[Route("{id:guid}/finish-event")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<ActionResult> FinishEvent(Guid id, FinishEventViewModel model)
		{
			try
			{

				// Set the event id to the model and ensure it's valid.
				model.EventId = id;
				if (!ModelState.IsValid)
				{
					return View(model);
				}
				
				// Parse the finish date
				string finishDateComplete = String.Format("{0} {1}:00", model.FinishDate, model.FinishTime);
				DateTime finishDate = DateTime.ParseExact(finishDateComplete, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).ToUniversalTime();

				// Finish the event
				await EventService.FinishEvent(id, finishDate);

				// Redirect to the event
				return new RedirectResult(String.Format("/events/{0}?finished=1", id));

			}
			catch (Exception ex)
			{
				await ServiceLocator.Get<ILogger>().Error(String.Format("Unable to finish event. Message: {0}", ex.Message), ex);
				return new RedirectResult(String.Format("/events/{0}?error_finishing=1", id));
			}

		}

		#endregion

		#region Crews

		[Route("{eventId:guid}/create-crew")]
		public async Task<ActionResult> CreateCrew(Guid eventId)
		{
			// Get the event based on the id
			Event eventObj = await EventService.GetById(eventId);

			// If the job is null, return 404
			if (eventObj == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Create the model
			CreateEditCrewViewModel model = new CreateEditCrewViewModel()
			{
				EventId = eventId
			};

			// Get the list of currently assigned users
			IList<Guid> assignedUserIds = eventObj.Crews.SelectMany(i => i.CrewMembers).ToList();

			// Load the users for the model
			IList<IdentityUser> users = await UnitService.GetUsersForUnit(eventObj.UnitId);
			foreach (IdentityUser user in users)
			{

				// If the user id is contained within the already assigned user ids, then skip to the next one
				if (assignedUserIds.Contains(user.Id))
				{
					continue;
				}

				model.AvailableMembers.Add(new Tuple<Guid, string, string>(user.Id, user.FullName, user.Profile.MemberNumber));
			}

			// return the view
			return View("CreateEditCrew", model);

		}

		[Route("{eventId:guid}/create-crew")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> CreateCrew(Guid eventId, CreateEditCrewViewModel model)
		{
			// Get the event based on the id
			Event eventObj = await EventService.GetById(eventId);

			// If the job is null, return 404
			if (eventObj == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Set the model event id
			model.EventId = eventId;

			// Get the list of currently assigned users
			IList<Guid> assignedUserIds = eventObj.Crews.SelectMany(i => i.CrewMembers).ToList();

			// Load the users for the model
			IList<IdentityUser> users = await UnitService.GetUsersForUnit(eventObj.UnitId);
			foreach (IdentityUser user in users)
			{

				// If the user id is contained within the already assigned user ids, then skip to the next one
				if (assignedUserIds.Contains(user.Id))
				{
					continue;
				}

				model.AvailableMembers.Add(new Tuple<Guid, string, string>(user.Id, user.FullName, user.Profile.MemberNumber));
			}

			// If the crew leader id is empty guid, or there is no crew members, it's an invalid model response
			if (String.IsNullOrEmpty(model.SelectedMembers))
			{
				ModelState.AddModelError("", "You need to select members to add to a crew.");
				return View("CreateEditCrew", model);
			}
			if (model.CrewLeaderId == Guid.Empty)
			{
				ModelState.AddModelError("", "You need to specify a crew leader for a crew.");
				return View("CreateEditCrew", model);
			}

			// If there is a crew with that name already, then return error
			if (eventObj.Crews.Any(i => i.Name.Equals(model.Name, StringComparison.CurrentCultureIgnoreCase)))
			{
				ModelState.AddModelError("", String.Format("There is already a crew named '{0}'. Crew names must be unique within an event.", model.Name));
				return View("CreateEditCrew", model);
			}

			try
			{

				// Create the list of crew members
				IList<Guid> crewMembers = new List<Guid>();

				// Get the list of crew members from the selected crew members
				crewMembers = model.SelectedMembers.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(i => new Guid(i)).ToList();

				// Create the crew
				Crew newCrew = await EventService.CreateCrew(eventId, model.Name, crewMembers, model.CrewLeaderId);

				// return the success result
				return new RedirectResult(String.Format("/events/{0}#crews", eventId));

			}
			catch (Exception ex)
			{
				// Log the exception and return failed result
				await Log.Error(String.Format("Error creating crew for event. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "There was an error creating the crew for the event.");
				return View("CreateEditCrew", model);
			}

		}

		[Route("{eventId:guid}/crews/{crewId:guid}")]
		public async Task<ActionResult> EditCrew(Guid eventId, Guid crewId)
		{
			// Get the event based on the id
			Event eventObj = await EventService.GetById(eventId);

			// If the job is null, return 404
			if (eventObj == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Get the crew based on the id
			Crew crew = eventObj.Crews.FirstOrDefault(i => i.Id == crewId);

			// If the crew is null, return 404
			if (crew == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Create the model
			CreateEditCrewViewModel model = new CreateEditCrewViewModel()
			{
				EventId = eventId,
				CrewId = crewId,
				CrewLeaderId = crew.CrewLeaderId,
				SelectedMembers = String.Join("|", crew.CrewMembers),
				Name = crew.Name
			};

			// If there is a value in the selected members, we need to append a trailing |
			if (!String.IsNullOrEmpty(model.SelectedMembers))
			{
				model.SelectedMembers = String.Format("{0}|", model.SelectedMembers);
			}

			// Get the list of currently assigned users, except for the members of the current crew
			IList<Guid> assignedUserIds = eventObj.Crews.SelectMany(i => i.CrewMembers).Except(crew.CrewMembers).ToList();

			// Load the users for the model
			IList<IdentityUser> users = await UnitService.GetUsersForUnit(eventObj.UnitId);
			foreach (IdentityUser user in users)
			{

				// If the user id is contained within the already assigned user ids, then skip to the next one
				if (assignedUserIds.Contains(user.Id))
				{
					continue;
				}

				model.AvailableMembers.Add(new Tuple<Guid, string, string>(user.Id, user.FullName, user.Profile.MemberNumber));
			}

			// return the view
			return View("CreateEditCrew", model);

		}

		[Route("{eventId:guid}/crews/{crewId:guid}")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> EditCrew(Guid eventId, Guid crewId, CreateEditCrewViewModel model)
		{
			// Get the event based on the id
			Event eventObj = await EventService.GetById(eventId);

			// If the job is null, return 404
			if (eventObj == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Get the crew based on the id
			Crew crew = eventObj.Crews.FirstOrDefault(i => i.Id == crewId);

			// If the crew is null, return 404
			if (crew == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Set the model properties
			model.EventId = eventId;
			model.CrewId = crewId;

			// Get the list of currently assigned users, except for the members of the current crew
			IList<Guid> assignedUserIds = eventObj.Crews.SelectMany(i => i.CrewMembers).Except(crew.CrewMembers).ToList();

			// Load the users for the model
			IList<IdentityUser> users = await UnitService.GetUsersForUnit(eventObj.UnitId);
			foreach (IdentityUser user in users)
			{

				// If the user id is contained within the already assigned user ids, then skip to the next one
				if (assignedUserIds.Contains(user.Id))
				{
					continue;
				}

				model.AvailableMembers.Add(new Tuple<Guid, string, string>(user.Id, user.FullName, user.Profile.MemberNumber));
			}

			// If the crew leader id is empty guid, or there is no crew members, it's an invalid model response
			if (String.IsNullOrEmpty(model.SelectedMembers))
			{
				ModelState.AddModelError("", "You need to select members to add to a crew.");
				return View("CreateEditCrew", model);
			}
			if (model.CrewLeaderId == Guid.Empty)
			{
				ModelState.AddModelError("", "You need to specify a crew leader for a crew.");
				return View("CreateEditCrew", model);
			}

			// If there is a crew with that name already, then return error
			if (eventObj.Crews.Where(i => i.Id != crewId).Any(i => i.Name.Equals(model.Name, StringComparison.CurrentCultureIgnoreCase)))
			{
				ModelState.AddModelError("", String.Format("There is already a crew named '{0}'. Crew names must be unique within an event.", model.Name));
				return View("CreateEditCrew", model);
			}

			try
			{

				// Create the list of crew members
				IList<Guid> crewMembers = new List<Guid>();

				// Get the list of crew members from the selected crew members
				crewMembers = model.SelectedMembers.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(i => new Guid(i)).ToList();

				// Create the crew
				crew.Name = model.Name;
				crew.CrewLeaderId = model.CrewLeaderId;
				crew.CrewMembers = crewMembers;

				// Save the crew
				await EventService.SaveCrew(eventId, crew);

				// return the success result
				return new RedirectResult(String.Format("/events/{0}#crews", eventId));

			}
			catch (Exception ex)
			{
				// Log the exception and return failed result
				await Log.Error(String.Format("Error creating crew for event. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "There was an error creating the crew for the event.");
				return View("CreateEditCrew", model);
			}

		}

		#endregion

		#region Helper methods

		/// <summary>
		/// Gets the jobs for the specific list of job message ids.
		/// </summary>
		/// <param name="jobMessageIds"></param>
		/// <returns></returns>
		private async Task<IList<EventJobViewModel>> GetJobsForEvent(IList<Guid> jobMessageIds, Event eventObj)
		{

			// Get the messages for the capcodes
			IList<JobMessage> jobMessages = await JobMessageService.GetByIds(jobMessageIds);

			// Get the list of event jobs
			IList<EventJobViewModel> eventJobs = jobMessages.Select(i => EventJobViewModel.FromJobMessage(i)).ToList();

			// Get all jobs ids currently assigned to crews
			IList<Guid> assignedJobIds = eventObj.Crews.SelectMany(i => i.JobMessageIds).ToList();

			// Determine jobs that have been assigned
			for (int i = 0; i < eventJobs.Count; i++)
			{
				// If the id is contained within the assigned jobs, then mark it as assigned
				if (assignedJobIds.Contains(eventJobs[i].Id))
				{
					eventJobs[i].Assigned = true;
				}
			}

			// return the event jobs
			return eventJobs;
		}

		/// <summary>
		/// Gets the crews model to add to the event view model.
		/// </summary>
		/// <param name="eventObj"></param>
		/// <returns></returns>
		private async Task<IList<CrewViewModel>> GetCrewsModelForEvent(Event eventObj, IList<Tuple<Guid, string, string>> availableMembers)
		{

			IList<CrewViewModel> crews = new List<CrewViewModel>();

			if (eventObj.Crews == null || eventObj.Crews.Count == 0)
			{
				return crews;
			}

			List<Guid> memberIds = new List<Guid>();
			// Add the crew leaders
			memberIds.AddRange(eventObj.Crews.Select(i => i.CrewLeaderId));

			// Add the crew members, excluding the crew leader user.
			memberIds.AddRange(eventObj.Crews.SelectMany(i => i.CrewMembers));

			// Remove any duplicates
			memberIds = memberIds.Distinct().ToList();

			// Get the users with the specific ids
			IList<IdentityUser> members = await UserService.GetUsersByIds(memberIds);

			// Loop through the crews in the 
			foreach (Crew crew in eventObj.Crews)
			{

				// Create the model
				CrewViewModel crewModel = new CrewViewModel()
				{
					Id = crew.Id,
					Created = crew.Created,
					Name = crew.Name,
					Updated = crew.Updated,
					CrewLeader = UnitMemberViewModel.FromIdentityUserWithoutRole(members.FirstOrDefault(i => i.Id == crew.CrewLeaderId)),
					CrewMembers = members.Where(i => crew.CrewMembers.Where(x => x != crew.CrewLeaderId).Contains(i.Id)).Select(i => UnitMemberViewModel.FromIdentityUserWithoutRole(i)).ToList()
				};

				// Add the model to the list of crew models
				crews.Add(crewModel);

			}

			// return the crews list
			return crews;
		}

		/// <summary>
		/// Gets a list of select list items that represents the current users available units.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		private async Task<IList<SelectListItem>> GetAvailableUnits(Guid userId)
		{
			// Get the list of units for the user
			IList<Unit> userUnits = await UnitService.GetUnitsForUser(userId);

			// Create the list of select list items
			IList<SelectListItem> availableUnits = new List<SelectListItem>()
			{
				new SelectListItem() { Text = "Please select...", Value = "" }
			};

			// Loop through the units and add to the list of available units
			foreach (Unit unit in userUnits)
			{
				availableUnits.Add(new SelectListItem()
				{
					Text = unit.Name,
					Value = unit.Id.ToString()
				});
			}

			// return the available units
			return availableUnits;
		}

		private async Task<IList<SelectListItem>> GetAvailableAgencies()
		{
			// Create the list of select list items
			IList<SelectListItem> items = new List<SelectListItem>();

			// Add please select
			items.Add(new SelectListItem() { Value = "", Text = "Please select..." });

			// Get all the agencies
			IList<Agency> agencies = await AgencyService.GetAll();

			// if the agencies list is not null, loop through and add item
			if (agencies != null)
			{
				foreach(Agency agency in agencies)
				{
					items.Add(new SelectListItem() { Value = agency.Id.ToString(), Text = agency.Name });
				}
			}

			// return the select list items
			return items;
		}

		#endregion

	}
}