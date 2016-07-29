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
using Enivate.ResponseHub.Model.RadarImages.Interface;
using Enivate.ResponseHub.Model.Warnings;
using Enivate.ResponseHub.Model.Warnings.Interface;
using Enivate.ResponseHub.UI.Models.Messages;
using Enivate.ResponseHub.UI.Models.Wallboard;
using Enivate.ResponseHub.Model.Messages.Interface;

namespace Enivate.ResponseHub.UI.Controllers
{
	[RoutePrefix("wallboard")]
    public class WallboardController : BaseController
	{

		protected IWarningService WarningService
		{
			get
			{
				return ServiceLocator.Get<IWarningService>();
			}
		}
		
		protected IRadarImageService RadarImageService
		{
			get
			{
				return ServiceLocator.Get<IRadarImageService>();
			}
		}

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
			IList<JobMessage> jobMessages = await JobMessageService.GetMostRecent(capcodes, MessageType.Job, 30);

			// Create the jobs list view model.
			JobMessageListViewModel jobListModel = await BaseJobsMessagesController.CreateJobMessageListModel(capcodes, jobMessages);
			model.Messages = jobListModel.Messages;

			// Get the warnings
			try
			{
				IList<IWarning> warnings = WarningService.GetWarnings(WarningSource.CountryFireAuthority | WarningSource.StateEmergencyService);
			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Error loading warnings. Message: {0}", ex.Message), ex);
				ViewBag.LoadWarningsError = true;
			}

			model.RadarImageFiles = RadarImageService.GetRadarImagesForProduct("IDR022");

			return View(model);
        }
    }
}