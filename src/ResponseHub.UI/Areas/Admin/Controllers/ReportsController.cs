using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.UI.Areas.Admin.Models.Reports;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("reports")]
	[ClaimsAuthorize(Roles = RoleTypes.SystemAdministrator)]
	public class ReportsController : Controller
    {



		private IGroupService _groupService;
		protected IGroupService GroupService
		{
			get
			{
				return _groupService ?? (_groupService = UnityConfiguration.Container.Resolve<IGroupService>());
			}
		}

		private ICapcodeService _capcodeService;
		protected ICapcodeService CapcodeService
		{
			get
			{
				return _capcodeService ?? (_capcodeService = UnityConfiguration.Container.Resolve<ICapcodeService>());
			}
		}

		// GET: Admin/Reports
		[Route()]
		public ActionResult Index()
        {
            return View();
        }

		[Route("groups-capcodes")]
		public async Task<ActionResult> GroupsCapcodes()
		{

			// Get all the groups
			IList<Group> allGroups = await GroupService.GetAll();

			// Get all the capcodes
			IList<Capcode> allCapcodes = await CapcodeService.GetAll();

			// Create the report view model
			IList<GroupsCapcodesReportViewModel> reportItems = new List<GroupsCapcodesReportViewModel>();

			// Iterate through all the groups
			foreach(Group group in allGroups)
			{
				// Create the report item and add to the list
				GroupsCapcodesReportViewModel reportItem = new GroupsCapcodesReportViewModel()
				{
					Id = group.Id.ToString(),
					GroupCapcode = group.Capcode,
					GroupName = group.Name,
					Service = group.Service.GetEnumDescription(),
					Capcodes = allCapcodes.Where(i => group.AdditionalCapcodes.Contains(i.Id)).ToList()
				};
				reportItems.Add(reportItem);
			}

			return View(reportItems);

		}

    }

}