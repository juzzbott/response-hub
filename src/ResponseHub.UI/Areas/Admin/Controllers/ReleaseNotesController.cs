using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

//using HeyRed.MarkdownSharp;
using MarkdownDeep;

using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("release-notes")]
	[ClaimsAuthorize(Roles = RoleTypes.SystemAdministrator)]
	public class ReleaseNotesController : Controller
    {
		// GET: Admin/ReleaseNotes
		[Route]
		public ActionResult Index()
        {

			// Load the markdown from the release notes file
			string markdown = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/release_notes.md"));

			// Convert the markdown to html
			Markdown md = new Markdown();
			md.ExtraMode = true;
			md.SafeMode = false;
			string releaseNotes = md.Transform(markdown);

			// Set the release notes
			ViewBag.ReleaseNotes = releaseNotes;

            return View();
        }
    }
}