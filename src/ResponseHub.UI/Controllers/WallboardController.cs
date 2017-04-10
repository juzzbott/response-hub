using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.UI.Models.Messages;
using Enivate.ResponseHub.UI.Models.Wallboard;
using Enivate.ResponseHub.Model.Messages.Interface;

namespace Enivate.ResponseHub.UI.Controllers
{
	[RoutePrefix("wallboard")]
    public class WallboardController : BaseController
	{

		protected ICapcodeService CapcodeService
		{
			get
			{
				return ServiceLocator.Get<ICapcodeService>();
			}
		}

		protected IJobMessageService JobMessageService
		{
			get
			{
				return ServiceLocator.Get<IJobMessageService>();
			}
		}

		[Route]
		public async Task<ActionResult> Index()
        {

			WallboardViewModel model = new WallboardViewModel();
			
			// Get the capcodes for the current user
			IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(UserId);

			// Get the messages for the capcodes
			IList<JobMessage> jobMessages = await JobMessageService.GetMostRecent(capcodes, MessageType.Job, 30, 0);

			// Create the jobs list view model.
			JobsController jobsController = new JobsController();
			JobMessageListViewModel jobListModel = await jobsController.CreateJobMessageListModel(capcodes, jobMessages);
			model.Messages = jobListModel.Messages;
			
			return View(model);
        }
    }
}