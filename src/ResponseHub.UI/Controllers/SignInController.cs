using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.UI.Models.SignIn;
using Enivate.ResponseHub.Model.SignIn;
using Enivate.ResponseHub.Model.SignIn.Interface;
using System.Net;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Common.Extensions;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("sign-in")]
    public class SignInController : BaseController
	{

		protected readonly ICapcodeService CapcodeService = ServiceLocator.Get<ICapcodeService>();
		protected readonly IUnitService UnitService = ServiceLocator.Get<IUnitService>();
		protected readonly IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();
		protected readonly ISignInEntryService SignInService = ServiceLocator.Get<ISignInEntryService>();

		// GET: Sign-in
		[Route]
		public async Task<ActionResult> Index()
		{
			
			// Get the initial model
			SignInViewModel model = await GetSignInModel(false, false);

			// Set the user id, as we are forcing it to be our current logged in user.
			model.UserId = UserId;

			return View(model);
        }

		[Route]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Index(SignInViewModel model)
		{
			
			// Ensure the model state is valid before proceeding
			if (!ModelState.IsValid)
			{
				return View("Index", model);
			}

			// return the sign in result
			return await GetSignInResult(model, UserId, false);
		}
		
		[Route("complete")]	
		public ActionResult SignInComplete()
		{
			return View();
		}

		[Route("unit-sign-in")]
		public async Task<ActionResult> UnitSignIn()
		{

			// Get the initial model
			SignInViewModel model = await GetSignInModel(true, false);

			return View("Index", model);
		}

		[Route("unit-sign-in")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> UnitSignIn(SignInViewModel model)
		{
			
			// Ensure the model state is valid before proceeding
			if (!ModelState.IsValid)
			{
				return View("Index", model);
			}

			// return the sign in result
			return await GetSignInResult(model, model.UserId, true);
		}

		[Route("sign-out")]
		public async Task<ActionResult> SignOut()
		{

			// Cerate the model
			SignOutViewModel model = new SignOutViewModel()
			{
				SignOutDate = DateTime.Now.ToString("yyyy-MM-dd"),
				SignOutTime = DateTime.Now.ToString("HH:mm")
			};

			// Get the sign ins for the user where there is no sign outs
			IList<SignInEntry> signIns = await SignInService.GetSignInsWithoutSignOutsForUser(UserId);

			// Map the sign ins
			foreach(SignInEntry signIn in signIns)
			{
				model.SignIns.Add(new SignInEntryListItemViewModel() {
					Id = signIn.Id,
					SignInTime = signIn.SignInTime,
					SignInType = signIn.SignInType.GetEnumDescription(),
					Description = (signIn.SignInType == SignInType.Operation ? signIn.OperationDetails.Description : "Training")
				});
			}


			return View(model);
		}

		[Route("sign-out/{id:guid}")]
		public async Task<ActionResult> SignOut(Guid id, SignOutViewModel model)
		{

			// If the model state is not valid, return
			if (!ModelState.IsValid)
			{
				
				// Get the sign ins for the user where there is no sign outs
				IList<SignInEntry> signIns = await SignInService.GetSignInsWithoutSignOutsForUser(UserId);

				// Map the sign ins
				foreach (SignInEntry signIn in signIns)
				{
					model.SignIns.Add(new SignInEntryListItemViewModel()
					{
						Id = signIn.Id,
						SignInTime = signIn.SignInTime,
						SignInType = signIn.SignInType.GetEnumDescription(),
						Description = (signIn.SignInType == SignInType.Operation ? signIn.OperationDetails.Description : "Training")
					});
				}

				return View("SignOut", model);
			}


			// Get the dateTime from the model
			DateTime signOutTime = DateTime.ParseExact(String.Format("{0} {1}", model.SignOutDate, model.SignOutTime), "yyyy-MM-dd HH:mm", null).ToUniversalTime();

			// Sign the user out
			await SignInService.SignUserOut(id, signOutTime);

			return new RedirectResult("/sign-in/sign-out?signed_out=1");
		}

		#region Helpers

		/// <summary>
		/// Gets the action result for the sign in result.
		/// </summary>
		/// <param name="model">The model containing the sign in data.</param>
		/// <param name="specifiedUserId">The user id to sign in from. This should be either the current user id, or the user id selected from the model.</param>
		/// <returns></returns>
		private async Task<ActionResult> GetSignInResult(SignInViewModel model, Guid specifiedUserId, bool redirectBackToUnitSignIn)
		{
			// Get the dateTime from the model
			DateTime signInTime = DateTime.ParseExact(String.Format("{0} {1}", model.StartDate, model.StartTime), "yyyy-MM-dd HH:mm", null).ToUniversalTime();

			// Create the sign on entry from the model
			SignInEntry signOn = new SignInEntry()
			{
				UserId = specifiedUserId,
				UnitId = model.UnitId,
				SignInTime = signInTime,
				SignInType = model.SignInType
			};

			// Set the details for the operation
			if (model.SignInType == SignInType.Operation)
			{
				signOn.OperationDetails = new OperationActivity()
				{
					Description = model.OperationDescription,
					JobId = model.OperationJobId.Value
				};
			}

			// Set the details for the training activity
			if (model.SignInType == SignInType.Training)
			{
				signOn.TrainingDetails = new TrainingActivity()
				{
					Description = model.TrainingDescription
				};
			}

			// Set the details for the other sign in type
			if (model.SignInType == SignInType.Other)
			{
				signOn.OtherDetails = new OtherActivity()
				{
					OtherType = model.SignInTypeOther,
					OtherDescription = (model.SignInTypeOther == OtherSignInType.Other ? model.OtherTypeDescription : "")
				};
			}

			try
			{

				// Add the sign in to the database
				await SignInService.SignUserIn(signOn);

				// Redirect to sign in complete.
				if (redirectBackToUnitSignIn)
				{
					return new RedirectResult("/sign-in/unit-sign-in?sign_in_complete=1");
				}
				else
				{
					return new RedirectResult("/sign-in/complete");
				}

			}
			catch (Exception ex)
			{
				// Display and log the exception.
				ModelState.AddModelError("", "There was a system error signing you in.");
				await Log.Error(String.Format("Unable to sign user '{0}' in. Message: {1}", UserId, ex.Message), ex);
				return View(model);
			}
		}

		/// <summary>
		/// Gets the initial sign in view model
		/// </summary>
		/// <returns></returns>
		public async Task<SignInViewModel> GetSignInModel(bool setAvailableUsers, bool checkForSignOuts)
		{
			// Get the capcodes for the current user
			IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(UserId);

			// Get the messages for the capcodes
			IList<JobMessage> jobMessages = await JobMessageService.GetMostRecent(capcodes.Select(i => i.CapcodeAddress), MessageType.Job, 3, 0);

			// Create the dictionary of jobs
			IList<SignInOperationItem> availableOperations = new List<SignInOperationItem>();
			foreach (JobMessage message in jobMessages)
			{
				string description = message.MessageContent;

				// If the length is over 100 chars, truncate it
				if (description.Length > 100)
				{
					description = String.Format("{0}...", description.Substring(0, 100));
				}
				// Add the message to the list.
				availableOperations.Add(new SignInOperationItem()
				{
					JobId = message.Id,
					Description = description,
					JobNumber = message.JobNumber,
					Timestamp = message.Timestamp.ToLocalTime()
				});
			}
			
			// Get the units for the user
			IList<Unit> userUnits = await UnitService.GetUnitsForUser(UserId);

			// If there is no units, then return error
			if (userUnits == null || userUnits.Count == 0)
			{
				throw new HttpException((int)HttpStatusCode.InternalServerError, "No units available for the user.");
			}

			// build the model object
			SignInViewModel model = new SignInViewModel()
			{
				StartDate = DateTime.Now.ToString("yyyy-MM-dd"),
				StartTime = DateTime.Now.ToString("HH:mm"),
				AvailableOperations = availableOperations,
				UnitId = userUnits.First().Id
			};

			// If we need to set the available users, then do so
			if (setAvailableUsers)
			{


				// Get the first unit id and get all the users in that unit
				IList<IdentityUser> users = await UnitService.GetUsersForUnit(model.UnitId);

				// Add the users to the select list
				IList<SelectListItem> userItems = new List<SelectListItem>();
				foreach (IdentityUser user in users)
				{
					userItems.Add(new SelectListItem() { Text = user.FullName, Value = user.Id.ToString() });
				}

				// Set the available users
				model.AvailableUsers = userItems;
			}

			// If we need to check for sign outs, determine how many sign ins the current user has, where there is no sign out. If > 0, show message to sign out
			if (checkForSignOuts)
			{
				// Count sign ins without sign outs
				int nonSignedOutCount = await SignInService.CountSignOutsRequiredForUser(UserId);

				// Set the sign outs required flag
				model.SignOutRequired = nonSignedOutCount > 0;
			}

			// return the model
			return model;
		}

		#endregion

	}
}
