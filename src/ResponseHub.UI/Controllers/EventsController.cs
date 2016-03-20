﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Events.Interface;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.UI.Models.Events;
using Enivate.ResponseHub.Model.Events;
using System.Globalization;
using System.Net;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("events")]
	public class EventsController : BaseController
    {
		
		IGroupService GroupService
		{
			get
			{
				return ServiceLocator.Get<IGroupService>();
			}
		}

		IEventService EventService
		{
			get
			{
				return ServiceLocator.Get<IEventService>();
			}
		}

		// GET: Events
		[Route]
        public async Task<ActionResult> Index()
        {

			// Get the groups for the user
			IList<Group> usersGroups = await GroupService.GetGroupsForUser(UserId);

			List<Event> events = new List<Event>();

			// If there is no search term, return all results, otherwise return only those that match the search results.
			if (String.IsNullOrEmpty(Request.QueryString["q"]))
			{
				// Get the most recent groups
				events.AddRange(await EventService.GetEventsByGroup(usersGroups.Select(i => i.Id)));
			}
			else
			{
				events.AddRange(await EventService.FindByKeywords(Request.QueryString["q"], usersGroups.Select(i => i.Id)));
			}

			// Create the list of view model items
			IList<EventListItemViewModel> model = new List<EventListItemViewModel>();
			foreach(Event eventObj in events)
			{
				// Get the group
				Group eventGroup = usersGroups.First(i => i.Id == eventObj.GroupId);

				// Add the list item to the model list
				model.Add(new EventListItemViewModel() {
					Id = eventObj.Id,
					Name = eventObj.Name,
					GroupId = eventGroup.Id,
					GroupName = eventGroup.Name,
					StartDate = eventObj.EventStarted.ToLocalTime(),
					FinishDate = eventObj.EventFinished.ToLocalTime()
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

			// Set the available groups
			model.AvailableGroups = await GetAvailableGroups(UserId);
			model.Name = DateTime.Now.ToString("d MMMM yyyy");
			model.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
			model.StartTime = DateTime.Now.AddMinutes(-90).ToString("HH:mm");

			return View(model);
		}

		[Route("create")]
		[HttpPost]
		public async Task<ActionResult> Create(CreateEventViewModel model)
		{
			// Set the available groups
			model.AvailableGroups = await GetAvailableGroups(UserId);

			// If the model state is invalid, return the view with the model
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try {

				// Parse the start date
				string startDateComplete = String.Format("{0} {1}:00", model.StartDate, model.StartTime);
				DateTime startDate = DateTime.ParseExact(startDateComplete, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);

				// Create the new event
				Event newEvent = await EventService.CreateEvent(model.Name, model.GroupId, UserId, startDate);

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

		#region View event

		[Route("{id:guid}")]
		public async Task<ActionResult> ViewEvent(Guid id)
		{
			// Get the event based on the id
			Event theEvent = await EventService.GetById(id);

			// If the job is null, return 404
			if (theEvent == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Get the group If it's null, throw system error
			Group group = await GroupService.GetById(theEvent.GroupId);
			if (group == null)
			{
				throw new HttpException((int)HttpStatusCode.InternalServerError, "The group details count not be found.");
			}

			// Create the model
			EventViewModel model = new EventViewModel()
			{
				EventFinished = theEvent.EventFinished,
				EventStarted = theEvent.EventStarted,
				GroupId = theEvent.GroupId,
				GroupName = group.Name,
				Name = theEvent.Name
			};

			// return the view
			return View(model);
		}

		#endregion

		#region Helper methods

		/// <summary>
		/// Gets a list of select list items that represents the current users available groups.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		private async Task<IList<SelectListItem>> GetAvailableGroups(Guid userId)
		{
			// Get the list of groups for the user
			IList<Group> userGroups = await GroupService.GetGroupsForUser(userId);

			// Create the list of select list items
			IList<SelectListItem> availableGroups = new List<SelectListItem>()
			{
				new SelectListItem() { Text = "Please select...", Value = "" }
			};

			// Loop through the groups and add to the list of available groups
			foreach (Group group in userGroups)
			{
				availableGroups.Add(new SelectListItem()
				{
					Text = group.Name,
					Value = group.Id.ToString()
				});
			}

			// return the available groups
			return availableGroups;
		}

		#endregion

	}
}