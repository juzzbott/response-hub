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

using Enivate.ResponseHub.ApplicationServices.Identity;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;

using Enivate.ResponseHub.UI.Areas.Admin.Models.Groups;
using System.Net;

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

			IUserRepository userRepo = UnityConfiguration.Container.Resolve<IUserRepository>();
			UserService svc = new UserService(userRepo);

			Enivate.ResponseHub.Model.Identity.User user = new Model.Identity.User()
			{
				Created = DateTime.UtcNow,
				FirstName = "Test",
				Surname = "User",
				EmailAddress = "testuser@domain.com",
				UserName = "testuser@domain.com"
			};

			//await svc.CreateAsync(user);
			
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

		[Route("{id:guid}")]
		public async Task<ActionResult> ViewGroup(Guid id)
		{

			// Get the group
			Group group = await GroupService.GetById(id);

			// If the place is null, throw 404
			if (group == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
			}

			// Create the model for the single view
			SingleGroupViewModel model = new SingleGroupViewModel()
			{
				Name = group.Name,
				Description = group.Description,
				Service = group.Service
			};

			return View(model);


		}

	}
}