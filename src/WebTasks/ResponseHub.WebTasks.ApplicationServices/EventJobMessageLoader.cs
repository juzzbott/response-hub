using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Events;
using Enivate.ResponseHub.Model.Events.Interface;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.WebTasks.ApplicationServices
{
	public class EventJobMessageLoader
	{

		private IEventService _eventService;
		private IJobMessageService _jobMessageService;
		private IUnitService _unitService;
		private ICapcodeService _capcodeService;
		private ILogger _log;

		public EventJobMessageLoader(IEventService eventService, IJobMessageService jobMessageService, IUnitService unitService, ICapcodeService capcodeService, ILogger log)
		{
			_eventService = eventService;
			_jobMessageService = jobMessageService;
			_unitService = unitService;
			_capcodeService = capcodeService;
			_log = log;
		}

		/// <summary>
		/// Iterates through all the active events and assigned any jobs within the start range to the events. 
		/// </summary>
		/// <returns></returns>
		public async Task SetJobMessagesForActiveEvents()
		{

			// Get all events that are open
			IList<Event> activeEvents = await _eventService.GetActiveEvents();

			// Log how many active events we found.
			await _log.Debug(String.Format("Found '{0}' active events. Processing...", activeEvents.Count));

			// Loop through the active events
			foreach(Event activeEvent in activeEvents)
			{

				// Log processing of event.
				await _log.Debug(String.Format("Processing event: {0} ({1}).", activeEvent.Name, activeEvent.Id));

				// Get the unit for the event
				Unit eventUnit = await _unitService.GetById(activeEvent.UnitId);

				if (eventUnit == null)
				{
					await _log.Error(String.Format("The unit with id: '{0}' could not be found.", activeEvent.UnitId));
				}

				// Get the capcodes for the unit
				IList<Capcode> unitCapcodes = await _capcodeService.GetCapcodesForUnit(eventUnit.Id);

				// Get the job message ids within the start date and an hour in the future
				IList<Guid> jobMessageIds = await _jobMessageService.GetMessageIdsBetweenDates(unitCapcodes, MessageType.Job, activeEvent.StartDate, DateTime.UtcNow.AddHours(1));

				// Now that we have the job message ids, we need to set them to the event
				await _eventService.SetJobsToEvent(activeEvent.Id, jobMessageIds);

				// Log how many active events we found.
				await _log.Debug(String.Format("Assigned '{0}' jobs to event: {1} ({2}).", jobMessageIds.Count, activeEvent.Name, activeEvent.Id));

			}

		}

	}
}
