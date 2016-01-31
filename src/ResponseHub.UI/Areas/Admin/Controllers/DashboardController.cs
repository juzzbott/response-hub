using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("dashboard")]
	public class DashboardController : Controller
    {

        // GET: Admin/Dashboard
		[Route]
        public ActionResult Index()
        {
            return View();
        }
    }
}