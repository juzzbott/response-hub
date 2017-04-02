using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Filters;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{
	[RouteArea("ControlPanel", AreaPrefix = "control-panel")]
	[RoutePrefix("resend-activation-email")]
	[ClaimsAuthorize(Roles = RoleTypes.GroupAdministrator)]
	public class ResendActivationEmailController : BaseControlPanelController
	{

		[Route("{id:guid}")]
        // GET: ControlPanel/ResendActivationEmail
        public async Task<ActionResult> Index(Guid id)
        {

			// Resend the activation email
			return await ResendActivationEmail(id);
        }

		[Route("complete")]
		public ActionResult Complete()
		{
			return View();
		}

    }
}