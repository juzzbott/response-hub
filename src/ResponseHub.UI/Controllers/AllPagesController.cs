using Enivate.ResponseHub.Model.Messages.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("all-pages")]
    public class AllPagesController : Controller
    {

		private IJobMessageService _jobMessageService;
		protected IJobMessageService JobMessageService
		{
			get
			{
				return _jobMessageService ?? (_jobMessageService = UnityConfiguration.Container.Resolve<IJobMessageService>());
			}
		}

		// GET: AllPages
		[Route()]
        public async Task<ActionResult> Index()
        {

			// Get the 50 most recent messages
			IList<JobMessage> messages = await JobMessageService.GetMostRecent(50, 0);

            return View(messages);
        }
    }
}