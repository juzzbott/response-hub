using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Caching;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.UI.Areas.Admin.Models.SystemSettings;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("system-settings")]
	[ClaimsAuthorize(Roles = RoleTypes.SystemAdministrator)]
	public class SystemSettingsController : Controller
    {

		[Route]
        // GET: Admin/SystemSettings
        public ActionResult Index()
        {

			// Create the view model
			SystemSettingsViewModel model = new SystemSettingsViewModel();
			model.Cache.TotalItems = CacheManager.GetItemCount();
			model.Cache.CacheKeys = CacheManager.GetCacheKeysWithExpiry();
			// Convert to MB
			model.Cache.CacheMemoryLimit = (CacheManager.CacheMemoryLimit() / 1000000);
			model.Cache.PysicalMemoryLimit = CacheManager.CachePhysicalMemoryLimit();
			model.Cache.PollingInterval = CacheManager.PollingInterval();

			return View(model);
        }

		[Route("clear-cache")]
		public ActionResult ClearCache()
		{
			// Clear the cache
			CacheManager.ClearCache();

			// redirect back to the system settings page
			return new RedirectResult("/admin/system-settings");

		}
    }
}