using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Controllers;
using Enivate.ResponseHub.UI.Filters;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[ClaimsAuthorize(Roles = RoleTypes.SystemAdministrator)]
	public class HomeController : BaseController
    {
        // GET: Admin/Home
		[Route]
        public ActionResult Index()
        {
			return new RedirectResult("/admin/units");
        }
    }
}