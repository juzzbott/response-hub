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

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("events")]
	public class EventsController : BaseController
    {
		
		IUnitService UnitService
		{
			get
			{
				return ServiceLocator.Get<IUnitService>();
			}
		}

		IEventService EventService
		{
			get
			{
				return ServiceLocator.Get<IEventService>();
			}
		}

		IAgencyService AgencyService
		{
			get
			{
				return ServiceLocator.Get<IAgencyService>();
			}
		}

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
				events.AddRange(await EventService.GetEventsByUnit(usersUnits.Select(i => i.Id)));
			}
			else
			{
				events.AddRange(await EventService.FindByKeywords(Request.QueryString["q"], usersUnits.Select(i => i.Id)));
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

			// Set the available units
			model.AvailableUnits = await GetAvailableUnits(UserId);
			model.Name = DateTime.Now.ToString("d MMMM yyyy");
			model.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
			model.StartTime = DateTime.Now.AddMinutes(-90).ToString("HH:mm");

			return View(model);
		}

		[Route("create")]
		[HttpPost]
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
				DateTime startDate = DateTime.ParseExact(startDateComplete, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);

				// Create the new event
				Event newEvent = await EventService.CreateEvent(model.Name, model.UnitId, UserId, startDate);

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
				EventFinished = eventObj.EventFinished,
				EventStarted = eventObj.EventStarted,
				UnitId = eventObj.UnitId,
				UnitName = unit.Name,
				Name = eventObj.Name
			};

			// Set the unit resources
			model.UnitResources = await GetUnitResources(unit.Id, eventObj);

			// Set the available resources
			model.AdditionalResourceModel.AvailableAgencies = await GetAvailableAgencies();

			// return the view
			return View(model);
		}

		#endregion

		#region Helper methods

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

		/// <summary>
		/// Gets the unit resources for the event. 
		/// </summary>
		/// <param name="unitId"></param>
		/// <returns></returns>
		private async Task<IList<EventResource>> GetUnitResources(Guid unitId, Event eventObj)
		{
			// Create the list of event resources
			IList<EventResource> resources = new List<EventResource>();

			// Get the unit
			Unit unit = await UnitService.GetById(unitId);

			// If the unit is null, return empty list
			if (unit == null)
			{
				return resources;
			}

			// Get the users for the specified unit
			IList<IdentityUser> users = await UnitService.GetUsersForUnit(unitId);

			// For each user in the unit, add the event resource
			foreach(IdentityUser user in users)
			{
				resources.Add(new EventResource() {
					Active = eventObj.Resources.Any(i => i.UserId.HasValue && i.UserId == user.Id),
					Name = user.FullName,
					Type = ResourceType.UnitMember,
					UserId = user.Id
				});
			}

			// return the list of resources 
			return resources;
			
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