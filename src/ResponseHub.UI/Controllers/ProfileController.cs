using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Models.Profile;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("profile")]
	public class ProfileController : BaseController
    {
		// GET: Profile
		[Route]
		public async Task<ActionResult> Index()
        {
			// Get the current user.
			IdentityUser user = await GetCurrentUser();

			// If there is a profile without an id, load the current users details
			return View("ViewProfile", MapUserToProfileModel(user));
		}

		// GET: Profile/{id}
		[Route("{id:guid}")]
		public async Task<ActionResult> ViewProfile(Guid id)
		{

			// Get the user based on the id.
			IdentityUser user = await UserService.FindByIdAsync(id);

			// If the user is null, then throw 404
			if (user == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}
			
			// return the view
			return View(MapUserToProfileModel(user));
		}

		/// <summary>
		/// Maps the identity user to the profile view model.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		private ProfileViewModel MapUserToProfileModel(IdentityUser user)
		{
			return new ProfileViewModel()
			{
				FirstName = user.FirstName,
				Surname = user.Surname,
				DateCreated = user.Created
			};
		}
	}
}