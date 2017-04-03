using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.UI.Models.SignIn;
using Enivate.ResponseHub.Model.SignIn;
using Enivate.ResponseHub.Model.SignIn.Interface;
using System.Net;
using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("sign-in")]
    public class SignInController : BaseController
	{

		protected readonly ICapcodeService CapcodeService = ServiceLocator.Get<ICapcodeService>();
		protected readonly IGroupService GroupService = ServiceLocator.Get<IGroupService>();
		protected readonly IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();
		protected readonly ISignInEntryService SignOnService = ServiceLocator.Get<ISignInEntryService>();

		// GET: Sign-in
		[Route]
		public async Task<ActionResult> Index()
		{
			
			// Get the initial model
			SignInViewModel model = await GetSignInModel(false);

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
			return await GetSignInResult(model, UserId);
		}

		

		[Route("complete")]	
		public ActionResult SignInComplete()
		{
			return View();
		}

		[Route("group-sign-in")]
		public async Task<ActionResult> GroupSignIn()
		{

			// Get the initial model
			SignInViewModel model = await GetSignInModel(true);

			return View("Index", model);
		}

		[Route("group-sign-in")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> GroupSignIn(SignInViewModel model)
		{
			
			// Ensure the model state is valid before proceeding
			if (!ModelState.IsValid)
			{
				return View("Index", model);
			}

			// return the sign in result
			return await GetSignInResult(model, model.GroupId);
		}

		#region Helpers

		/// <summary>
		/// Gets the action result for the sign in result.
		/// </summary>
		/// <param name="model">The model containing the sign in data.</param>
		/// <param name="specifiedUserId">The user id to sign in from. This should be either the current user id, or the user id selected from the model.</param>
		/// <returns></returns>
		private async Task<ActionResult> GetSignInResult(SignInViewModel model, Guid specifiedUserId)
		{
			// Get the dateTime from the model
			DateTime signInTime = DateTime.ParseExact(String.Format("{0} {1}", model.StartDate, model.StartTime), "yyyy-MM-dd HH:mm", null).ToUniversalTime();

			// Create the sign on entry from the model
			SignInEntry signOn = new SignInEntry()
			{
				UserId = specifiedUserId,
				GroupId = model.GroupId,
				SignInTime = signInTime,
				SignInType = model.SignOnType
			};

			// Set the specific 
			if (model.SignOnType == SignInType.Operation)
			{
				signOn.OperationDetails = new OperationActivity()
				{
					Description = model.OperationDescription,
					JobId = model.OperationJobId.Value
				};
			}

			try
			{

				// Add the sign in to the database
				await SignOnService.SignUserIn(signOn);

				// Redirect to sign in complete.
				return new RedirectResult("/sign-in/complete");

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
		public async Task<SignInViewModel> GetSignInModel(bool setAvailableUsers)
		{
			// Get the capcodes for the current user
			IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(UserId);

			// Get the messages for the capcodes
			IList<JobMessage> jobMessages = await JobMessageService.GetMostRecent(capcodes, MessageType.Job, 3);

			// Create the dictionary of jobs
			IList<Tuple<Guid, string, string>> availableOperations = new List<Tuple<Guid, string, string>>();
			foreach (JobMessage message in jobMessages)
			{
				string description = message.MessageContent;

				// If the length is over 100 chars, truncate it
				if (description.Length > 100)
				{
					description = String.Format("{0}...", description.Substring(0, 100));
				}
				// Add the message to the list.
				availableOperations.Add(new Tuple<Guid, string, string>(message.Id, description, message.JobNumber));
			}
			
			// Get the groups for the user
			IList<Group> userGroups = await GroupService.GetGroupsForUser(UserId);

			// If there is no groups, then return error
			if (userGroups == null || userGroups.Count == 0)
			{
				throw new HttpException((int)HttpStatusCode.InternalServerError, "No groups available for the user.");
			}

			// build the model object
			SignInViewModel model = new SignInViewModel()
			{
				StartDate = DateTime.Now.ToString("yyyy-MM-dd"),
				StartTime = DateTime.Now.ToString("HH:mm"),
				AvailableOperations = availableOperations,
				GroupId = userGroups.First().Id
			};

			// If we need to set the available users, then do so
			if (setAvailableUsers)
			{


				// Get the first group id and get all the users in that group
				IList<IdentityUser> users = await GroupService.GetUsersForGroup(model.GroupId);

				// Add the users to the select list
				IList<SelectListItem> userItems = new List<SelectListItem>();
				foreach (IdentityUser user in users)
				{
					userItems.Add(new SelectListItem() { Text = user.FullName, Value = user.Id.ToString() });
				}

				// Set the available users
				model.AvailableUsers = userItems;
			}

			// return the model
			return model;
		}

		#endregion

	}
}
