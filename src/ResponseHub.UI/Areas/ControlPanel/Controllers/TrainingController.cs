using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Training;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{

	[RouteArea("ControlPanel", AreaPrefix = "control-panel")]
	[RoutePrefix("training")]
	[ClaimsAuthorize(Roles = RoleTypes.GroupAdministrator)]
	public class TrainingController : BaseControlPanelController
    {
        // GET: ControlPanel/Training
		[Route]
        public ActionResult Index()
		{
			return View();
		}

		[Route("add")]
		public ActionResult Add()
		{
			AddTrainingSessionViewModel model = new AddTrainingSessionViewModel();
			return View(model);
		}

    }
}