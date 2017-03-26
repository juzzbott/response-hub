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

			// Get the groups for the current user
			IList<Group> userGroups =  await GroupService.GetGroupsForUser(UserId);

			// Get the capcodes for the current user
			IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(UserId);

			// Get the messages for the capcodes
			IList<JobMessage> jobMessages = await JobMessageService.GetMostRecent(capcodes, MessageType.Job, 3);

			// Create the dictionary of jobs
			IList<Tuple<Guid, string, string>> availableOperations = new List<Tuple<Guid, string, string>>();
			foreach(JobMessage message in jobMessages)
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

			SignInViewModel model = new SignInViewModel()
			{
				StartDate = DateTime.Now.ToString("yyyy-MM-dd"),
				StartTime = DateTime.Now.ToString("HH:mm"),
				AvailableOperations = availableOperations,
				UserId = UserId
			};
			
			// Create the available groups
			foreach (Group group in userGroups)
			{
				model.AvailableGroups.Add(new SelectListItem() { Text = group.Name, Value = group.Id.ToString() });
			}

			return View(model);
        }

		[Route]
		[HttpPost]
		public async Task<ActionResult> Index(SignInViewModel model)
		{
			
			// Get the groups for the current user
			IList<Group> userGroups = await GroupService.GetGroupsForUser(UserId);

			// Create the available groups
			model.AvailableGroups.Clear();
			foreach (Group group in userGroups)
			{
				model.AvailableGroups.Add(new SelectListItem() { Text = group.Name, Value = group.Id.ToString() });
			}

			// Get the dateTime from the model
			DateTime signInTime = DateTime.ParseExact(String.Format("{0} {1}", model.StartDate, model.StartTime), "yyyy-MM-dd HH:mm", null).ToUniversalTime();

			// Create the sign on entry from the model
			SignInEntry signOn = new SignInEntry()
			{
				UserId = UserId,
				GroupId = model.Group,
				SignInTime = signInTime,
				SignInType = model.SignOnType
			};

			// Set the specific 
			switch(model.SignOnType)
			{
				case SignInType.Operations:
					signOn.ActivityDetails = new OperationActivity()
					{
						Description = model.OperationDescription,
						JobId = model.OperationJobId.Value
					};
					break;

				case SignInType.Training:
					signOn.ActivityDetails = new TrainingActivity()
					{
						OtherDescription = model.TrainingTypeOther,
						TrainingType = model.TrainingType
					};
					break;
			}

			try
			{

				// Add the sign in to the database
				await SignOnService.SignUserIn(signOn);


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

		[Route("complete")]	
		public ActionResult SignInComplete()
		{
			return View();
		}

		[Route("group-sign-in")]
		public async Task<ActionResult> GroupSignIn()
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

			// Get the first group id and get all the users in that group
			Guid groupId = userGroups.Select(i => i.Id).FirstOrDefault();
			IList<IdentityUser> users = await GroupService.GetUsersForGroup(groupId);

			// Add the users to the select list
			IList<SelectListItem> userItems = new List<SelectListItem>();
			foreach(IdentityUser user in users)
			{
				userItems.Add(new SelectListItem() { Text = user.FullName, Value = user.Id.ToString() });
			}

			SignInViewModel model = new SignInViewModel()
			{
				StartDate = DateTime.Now.ToString("yyyy-MM-dd"),
				StartTime = DateTime.Now.ToString("HH:mm"),
				AvailableOperations = availableOperations,
				AvailableUsers = userItems
			};

			return View("Index", model);
		}

    }
}
