using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Controllers
{
    public class HomeController : Controller
    {

		// GET: Home
		[Route]
		public ActionResult Index()
        {
			return new RedirectResult("/jobs");
        }
    }
}