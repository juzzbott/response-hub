using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;

using Enivate.ResponseHub.UI.Areas.Admin.Models.Groups;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("groups")]
    public class GroupsController : Controller
    {

		private IGroupService _groupService;
		protected IGroupService GroupService
		{
			get
			{
				return _groupService ?? (_groupService = UnityConfiguration.Container.Resolve<IGroupService>());
			}
		}


		[Route]
        // GET: Admin/Groups
        public async Task<ActionResult> Index()
        {

			// Get the most recent groups
			IList<Group> recentGroups = await GroupService.GetRecentlyAdded(10);

            return View(recentGroups);
        }

		[Route("create")]
		public ActionResult Create()
		{
			CreateGroupModel model = new CreateGroupModel();
			return View(model);
		}

		[Route("create")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create(CreateGroupModel model)
		{

			// Create the group
			Group newGroup = await GroupService.CreateGroup(model.Name, model.Service, model.Description);

			return View(model);
		}

	}
}