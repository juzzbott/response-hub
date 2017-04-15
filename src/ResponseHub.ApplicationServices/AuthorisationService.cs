using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Events;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Units.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class AuthorisationService : IAuthorisationService
	{
		
		IUnitService UnitService
		{
			get
			{
				return ServiceLocator.Get<IUnitService>();
			}
		}

		/// <summary>
		/// Determines if the user id has access to edit the event or not. 
		/// </summary>
		/// <param name="eventObj">The event object to check if the user can edit. </param>
		/// <param name="userId">The id of the user to check.</param>
		/// <returns>True if the user can edit the event, otherwise false.</returns>
		public async Task<bool> CanEditEvent(Event eventObj, Guid userId)
		{
			// If the event is null, throw exception as we can't test a null event
			if (eventObj == null)
			{
				throw new ArgumentNullException("eventObj");
			}

			if (userId == Guid.Empty)
			{
				throw new Exception("User id must be a non-empty guid.");
			}

			// Get the unit for the event
			Unit eventUnit = await UnitService.GetById(eventObj.UnitId);

			// If the unit is null, throw ex as we can't validate the user
			if (eventUnit == null)
			{
				throw new Exception("Cannot check security for null unit.");
			}

			// Finally, check the user is a member of the unit
			return eventUnit.Users.Any(i => i.UserId == userId);

		}
	}
}
