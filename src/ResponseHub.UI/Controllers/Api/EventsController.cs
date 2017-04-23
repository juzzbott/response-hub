﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Agencies.Interface;
using Enivate.ResponseHub.Model.Events.Interface;
using Enivate.ResponseHub.Model.Crews;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Models.Api.Events;
using Enivate.ResponseHub.UI.Models.Events;
using Enivate.ResponseHub.UI.Models.Users;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.UI.Controllers.Api
{

	[RoutePrefix("api/events")]
	public class EventsController : BaseApiController
    {

		IEventService EventService = ServiceLocator.Get<IEventService>();
		IAgencyService AgencyService = ServiceLocator.Get<IAgencyService>();
		IAuthorisationService AuthorisationService = ServiceLocator.Get<IAuthorisationService>();
		IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();

		#region Crews

		[Route("{eventId:guid}/crews")]
		[HttpGet]

		public async Task<IList<Crew>> GetCrews(Guid eventId)
		{
			throw new NotImplementedException();
		}

		[Route("{eventId:guid}/crew/{crewId:guid}")]
		public async Task<CrewViewModel> GetCrew(Guid eventId, Guid crewId)
		{
			Crew crew = await EventService.GetCrewFromEvent(eventId, crewId);

			// If the crew is null, throw not found
			if (crew == null)
			{
				throw new HttpResponseException(HttpStatusCode.NotFound);
			}

			List<Guid> memberIds = new List<Guid>();

			// Add the crew leaders
			memberIds.Add(crew.CrewLeaderId);

			// Add the crew members, excluding the crew leader user.
			memberIds.AddRange(crew.CrewMembers);

			// Remove any duplicates
			memberIds = memberIds.Distinct().ToList();

			// Get the users with the specific ids
			IList<IdentityUser> members = await UserService.GetUsersByIds(memberIds);
			
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

			// Get the jobs
			IList<JobMessage> jobMessages = await JobMessageService.GetByIds(crew.JobMessageIds);

			// Map the jobs to the assigned job view model
			crewModel.AssignedJobs = jobMessages.Select(i => EventJobViewModel.FromJobMessage(i)).ToList();

			// return the crewModel
			return crewModel;

		}

		[Route("{eventId:guid}/crew/{crewId:guid}/assign-jobs")]
		public async Task AssignJobsToCrew(Guid eventId, Guid crewId, AssignJobsToCrewPostModel model)
		{

			// Assign the jobs to the crews
			await EventService.AssignJobsToCrew(eventId, crewId, model.JobMessageIds);

		}

		#endregion

		#region Helpers

		#endregion

	}
}
