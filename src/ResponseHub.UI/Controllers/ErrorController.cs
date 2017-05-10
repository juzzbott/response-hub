using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Enivate.ResponseHub.UI.Controllers
{
	[RoutePrefix("error")]
	[AllowAnonymous]
	public class ErrorController : Controller
	{

		[Route("not-found")]
		public ActionResult NotFound()
		{
			Response.StatusCode = 404;
			return View();
		}

		[Route("server-error")]
		public ActionResult ServerError()
		{
			Response.StatusCode = 500;
			return View();
		}

		[Route("forbidden")]
		public ActionResult Forbidden()
		{
			Response.StatusCode = 403;
			return View();
		}

	}
}