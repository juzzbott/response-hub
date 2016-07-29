using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Events;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class AuthorisationService : IAuthorisationService
	{
		
		IGroupService GroupService
		{
			get
			{
				return ServiceLocator.Get<IGroupService>();
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

			// Get the group for the event
			Group eventGroup = await GroupService.GetById(eventObj.GroupId);

			// If the group is null, throw ex as we can't validate the user
			if (eventGroup == null)
			{
				throw new Exception("Cannot check security for null group.");
			}

			// Finally, check the user is a member of the group
			return eventGroup.Users.Any(i => i.UserId == userId);

		}
	}
}
