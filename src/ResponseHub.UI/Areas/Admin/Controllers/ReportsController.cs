using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.UI.Areas.Admin.Models.Reports;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("reports")]
	[ClaimsAuthorize(Roles = RoleTypes.SystemAdministrator)]
	public class ReportsController : Controller
    {



		private IUnitService _unitService;
		protected IUnitService UnitService
		{
			get
			{
				return _unitService ?? (_unitService = UnityConfiguration.Container.Resolve<IUnitService>());
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

		[Route("unit-capcodes")]
		public async Task<ActionResult> UnitsCapcodes()
		{

			// Get all the unit
			IList<Unit> allUnits = await UnitService.GetAll();

			// Get all the capcodes
			IList<Capcode> allCapcodes = await CapcodeService.GetAll();

			// Create the report view model
			IList<UnitsCapcodesReportViewModel> reportItems = new List<UnitsCapcodesReportViewModel>();

			// Iterate through all the unit
			foreach(Unit unit in allUnits)
			{
				// Create the report item and add to the list
				UnitsCapcodesReportViewModel reportItem = new UnitsCapcodesReportViewModel()
				{
					Id = unit.Id.ToString(),
					UnitCapcode = unit.Capcode,
					UnitName = unit.Name,
					Service = unit.Service.GetEnumDescription(),
					Capcodes = allCapcodes.Where(i => unit.AdditionalCapcodes.Contains(i.Id)).ToList()
				};
				reportItems.Add(reportItem);
			}

			return View(reportItems);

		}

    }

}