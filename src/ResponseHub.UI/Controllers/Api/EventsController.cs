using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Agencies;
using Enivate.ResponseHub.Model.Agencies.Interface;
using Enivate.ResponseHub.Model.Events;
using Enivate.ResponseHub.Model.Events.Interface;
using Enivate.ResponseHub.UI.Models.Api.Events;
using Enivate.ResponseHub.Model.Crews;

namespace Enivate.ResponseHub.UI.Controllers.Api
{

	[RoutePrefix("api/events")]
	public class EventsController : BaseApiController
    {

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

		IAuthorisationService AuthorisationService
		{
			get
			{
				return ServiceLocator.Get<IAuthorisationService>();
			}
		}

		#region Resources

		[Route("{id:guid}/resources")]
		[HttpPost]
		public async Task<AddResourceResponseModel> AddResource(Guid id, AddResourcePostModel model)
		{
			// Get the event by the id
			Event eventObj = await EventService.GetById(id);
			
			// If the event is null, throw not found
			if (eventObj == null)
			{
				throw new HttpResponseException(HttpStatusCode.NotFound);
			}

			// Ensure the user can edit the event
			if (!await AuthorisationService.CanEditEvent(eventObj, UserId))
			{
				throw new HttpResponseException(HttpStatusCode.Forbidden);
			}

			try
			{

				// Add the resource to the event
				EventResource newResource = await EventService.AddResourceToEvent(id, model.Name, model.AgencyId, model.UserId, model.Type);

				// Get the agency
				Agency agency = await AgencyService.GetByID(model.AgencyId);

				// return the response model.
				return new AddResourceResponseModel()
				{
					AgencyId = model.AgencyId,
					AgencyName = agency.Name,
					Id = newResource.Id,
					Name = model.Name
				};

			}
			catch (Exception ex)
			{
				// Log the exception and throw http exception
				await Log.Error(String.Format("Unable to create new event resource. Message: {0}", ex.Message), ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}

		}

		#endregion

		#region Crews

		[Route("{id:guid}/crews")]
		[HttpGet]

		public async Task<IList<Crew>> GetCrews(Guid eventId)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Helpers


		#endregion

	}
}
