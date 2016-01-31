using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("home")]
	public class HomeController : Controller
    {
        // GET: Admin/Home
		[Route]
        public ActionResult Index()
        {
			return new RedirectResult("/admin/dashboard");
        }
    }
}