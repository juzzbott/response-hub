using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("messages")]
	public class MessagesController : Controller
	{

		private ILogger _log;
		protected ILogger Log
		{
			get
			{
				return _log ?? (_log = UnityConfiguration.Container.Resolve<ILogger>());
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

		private IJobMessageService _jobMessageService;
		protected IJobMessageService JobMessageService
		{
			get
			{
				return _jobMessageService ?? (_jobMessageService = UnityConfiguration.Container.Resolve<IJobMessageService>());
			}
		}

		// GET: Jobs
		[Route]
		public async Task<ActionResult> Index()
		{

			// Get the current user id
			Guid userId = new Guid(User.Identity.GetUserId());

			// Get the capcodes for the current user
			IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(userId);

			// Get the messages for the capcodes
			IList<JobMessage> messages = await JobMessageService.GetMostRecent(capcodes.Select(i => i.CapcodeAddress), 50, MessageType.Message);

			return View("~/Views/Jobs/Index.cshtml", messages);
		}

	}
}