using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.SignIn;
using Enivate.ResponseHub.Model.SignIn.Interface;
using Enivate.ResponseHub.UI.Models.Api.SignIn;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Controllers.Api
{
	[RoutePrefix("api/sign-in")]
	public class SignInController : BaseApiController
	{


		protected readonly ISignInEntryService SignInService = ServiceLocator.Get<ISignInEntryService>();
		protected readonly IUnitService UnitService = ServiceLocator.Get<IUnitService>();

		[Route]
		[HttpPost]
		public async Task<PostSignInResponse> Post(PostSignInModel model)
		{

			// Create the sign in
			try
			{

				// Get the units for the user
				IList<Unit> userUnits = await UnitService.GetUnitsForUser(UserId);

				// If there is no units, then return error
				if (userUnits == null || userUnits.Count == 0)
				{
					ThrowResponseException(HttpStatusCode.BadRequest, "No units available for the user.");
					return null;
				}

				// Create the sign in
				SignInEntry entry = new SignInEntry()
				{
					Created = DateTime.UtcNow,
					OperationDetails = new OperationActivity() { JobId = model.JobMessageId, Description = model.Description },
					SignInTime = DateTime.UtcNow,
					SignInType = model.SignInType,
					UserId = UserId,
					UnitId = userUnits.First().Id
				};

				// Sign the user in
				await SignInService.SignUserIn(entry);

				// Get the user details
				IdentityUser currentUser = await GetCurrentUser();

				// return the result
				return new PostSignInResponse()
				{
					SignInTime = entry.SignInTime,
					FullName = currentUser.FullName,
					MemberNumber = currentUser.Profile.MemberNumber
				};

			}
			catch (Exception ex)
			{
				// Log the error
				await Log.Error(String.Format("Error signing in user. Message: {0}", ex.Message), ex);
				ThrowResponseException(HttpStatusCode.InternalServerError);
				return null;
			}

		}

	}
}
