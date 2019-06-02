using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.UI.Models.Messages;
using System.Web;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.SignIn;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("messages")]
	public class MessagesController : BaseJobsMessagesController
	{

		// GET: Jobs
		[Route]
		public async Task<ActionResult> Index()
		{

			// Get the current user id
			Guid userId = new Guid(User.Identity.GetUserId());

			// Get the initial messages list
			JobMessageListViewModel model = await GetAllJobsMessagesViewModel(userId, MessageType.Message);

			return View("~/Views/Jobs/AllJobs.cshtml", model);
		}

		[Route("{id:guid}")]
		public async Task<ActionResult> ViewMessage(Guid id)
		{

			try
			{

				// Get the job message from the database
				JobMessage job = await JobMessageService.GetById(id);

				// If the job is null, return 404
				if (job == null)
				{
					throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
				}

                // Get the current user unit
                IList<Unit> userUnits = await UnitService.GetUnitsForUser(UserId);
                Unit unit = userUnits.First();

                // Get the capcode for the message
                Capcode capcode = await CapcodeService.GetByCapcodeAddress(unit.Capcode);

                // Get the sign ins for the job
                IList<SignInEntry> jobSignIns = await SignInEntryService.GetSignInsForJobMessage(job.Id);

				// Get the list of users who signed in for the job
				IList<IdentityUser> signInUsers = await UserService.GetUsersByIds(jobSignIns.Select(i => i.UserId));

				// Create the model object.
				JobMessageViewModel model = await MapJobMessageToViewModel(job, capcode.ToString(), jobSignIns, signInUsers, null, UserId);
				
				// return the job view
				return View(model);

			}
			catch (Exception ex)
			{
				// Log the error
				await Log.Error(String.Format("Unable to load the job details. Message: {0}", ex.Message, ex));
				return View(new object());

			}

		}

	}
}