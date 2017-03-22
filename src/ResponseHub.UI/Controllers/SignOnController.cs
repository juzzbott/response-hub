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
using Enivate.ResponseHub.UI.Models.SignOn;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("sign-on")]
    public class SignOnController : BaseController
	{

		protected readonly ICapcodeService CapcodeService = ServiceLocator.Get<ICapcodeService>();
		protected readonly IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();

		// GET: SignOn
		[Route]
		public async Task<ActionResult> Index()
		{

			// Get the current user id
			Guid userId = new Guid(User.Identity.GetUserId());

			// Get the capcodes for the current user
			IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(userId);

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

			SignOnViewModel model = new SignOnViewModel()
			{
				StartDate = DateTime.Now.ToString("yyyy-MM-dd"),
				StartTime = DateTime.Now.ToString("HH:mm"),
				AvailableOperations = availableOperations
			};

            return View(model);
        }

		[Route]
		[HttpPost]
		public ActionResult Index(SignOnViewModel model)
		{
			return View(model);
		}
    }
}