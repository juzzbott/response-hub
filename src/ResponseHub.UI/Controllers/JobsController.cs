using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Groups;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("jobs")]
    public class JobsController : Controller
	{

		private ICapcodeService _capcodeService;
		protected ICapcodeService CapcodeService
		{
			get
			{
				return _capcodeService ?? (_capcodeService = UnityConfiguration.Container.Resolve<ICapcodeService>());
			}
		}

		// GET: Jobs
		[Route]
        public async Task<ActionResult> Index()
        {

			// Get the capcodes for the current user
			IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(new Guid("02245CEE-22D3-8D40-9B36-68F22E6FB71D"));


			return View();
        }
    }
}