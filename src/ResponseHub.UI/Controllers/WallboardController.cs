using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Warnings.Interface;
using Enivate.ResponseHub.UI.Models.Messages;
using Enivate.ResponseHub.UI.Models.Wallboard;
using Enivate.ResponseHub.Model.Warnings;

namespace Enivate.ResponseHub.UI.Controllers
{
    public class WallboardController : Controller
    {

		private IWarningService _warningService;
		protected IWarningService WarningService
		{
			get
			{
				return _warningService ?? (_warningService = UnityConfiguration.Container.Resolve<IWarningService>());
			}
		}

		// GET: Wallboard
		public ActionResult Index()
        {

			WallboardViewModel model = new WallboardViewModel();
			model.Messages.Add(new ParsedMessageViewModel() {
				Capcode = "0001235",
				MessageBody = "ALERT F160203101 PARW1 RESCC1 * CAR ACCIDENT - POSS PERSON TRAPPED/FIRE CNR GLENMORE RD/NEROWIE RD PARWAN SVC 6608 E2 (747184) BACC1 CBMSH CEYNE CPARW [BACC]",
				CapcodeGroupId = Guid.NewGuid(),
				CapcodeGroupName = "Bacchus Marsh SES",
				JobNumber = "F160203101",
				Priority = MessagePriority.Emergency,
				Timestamp = DateTime.UtcNow
			});
			model.Messages.Add(new ParsedMessageViewModel()
			{
				Capcode = "0001235",
				MessageBody = "S160230607 SES BACCHUS MARSH TREE DOWN / TRF HAZARD JANELLE PROSSER 0439966838 / BALLAN-EGERTON RD MOUNT EGERTON /SOUTH MAIN RD //MANLEYS RD SVVB C 6523 J4 TREE BRANCH DOWN ONTO THE ROAD - COVERING ONE LANE [BACC]",
				CapcodeGroupId = Guid.NewGuid(),
				CapcodeGroupName = "Bacchus Marsh SES",
				JobNumber = "S160230607",
				Priority = MessagePriority.NonEmergency,
				Timestamp = DateTime.UtcNow.AddHours(-2)
			});
			model.Messages.Add(new ParsedMessageViewModel()
			{
				Capcode = "0001235",
				MessageBody = "members required for line search at 0800 tomorrow the 18th February for missing person contact Bacchus marsh duty officer if you are available duration all day. From Bacchus marsh duty officer. [BACC]",
				CapcodeGroupId = Guid.NewGuid(),
				CapcodeGroupName = "Bacchus Marsh SES",
				JobNumber = "",
				Priority = MessagePriority.Administration,
				Timestamp = DateTime.UtcNow.AddHours(-20)
			});

			// Get the warnings
			IList<IWarning> warnings = WarningService.GetWarnings(WarningSource.CountryFireAuthority | WarningSource.StateEmergencyService);

			return View(model);
        }
    }
}