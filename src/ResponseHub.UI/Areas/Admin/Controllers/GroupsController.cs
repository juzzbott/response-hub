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
	[Authorize(Roles = "System Administrator")]
    public class GroupsController : Controller
    {

		private const string CreateGroupViewModelSesionKey = "CreateGroupViewModel";

		private IGroupService _groupService;
		protected IGroupService GroupService
		{
			get
			{
				return _groupService ?? (_groupService = UnityConfiguration.Container.Resolve<IGroupService>());
			}
		}

		private IUserService _userService;
		protected IUserService UserService
		{
			get
			{
				return _userService ?? (_userService = UnityConfiguration.Container.Resolve<IUserService>());
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

			// Get the service type.
			int intServiceType;
			Int32.TryParse(model.Service, out intServiceType);
			ServiceType serviceType = (ServiceType)intServiceType;

			// Ensure the group name/service combination is unique
			bool groupExists = await GroupService.CheckIfGroupExists(model.Name, serviceType);

			// If the group exists, then display the group exists message
			if (groupExists)
			{
				ModelState.AddModelError("", "Sorry, there is already a group by that name in the selected service.");
				return View(model);
			}

			// Store the CreateGroupViewModel in session for the next screen
			Session[CreateGroupViewModelSesionKey] = model;

			// Redirect to the group administrator screen
			return new RedirectResult("/admin/groups/create/group-administrator");

			
			//// Create the group
			//Group newGroup = await GroupService.CreateGroup(model.Name, serviceType, model.Description);
			//
			//return View(model);
		}

		[Route("create/group-administrator")]
		public async Task<ActionResult> GroupAdministrator()
		{

			// Get the CreateGroupViewModel from session
			CreateGroupModel model = (CreateGroupModel)Session[CreateGroupViewModelSesionKey];

			// If the view model is null, redirect back to the create screen
			if (model == null)
			{
				return new RedirectResult("/admin/groups/create");
			}

			// Get the identity user related to the specified group admin
			IdentityUser groupAdminUser = await UserService.FindByEmailAsync(model.GroupAdministratorEmail);
			
			// If there is a group user, then add to the model.
			if (groupAdminUser != null)
			{
				model.GroupAdministrator.UserId = groupAdminUser.Id;
				model.GroupAdministrator.FirstName = groupAdminUser.FirstName;
				model.GroupAdministrator.Surname = groupAdminUser.Surname;
				model.GroupAdministrator.EmailAddress = model.GroupAdministratorEmail;
			}

			return View(model.GroupAdministrator);
		}

		[Route("create/group-administrator")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<ActionResult> GroupAdministrator(GroupAdministratorViewModel model)
		{
			// Get the CreateGroupViewModel from session
			CreateGroupModel createGroupModel = (CreateGroupModel)Session[CreateGroupViewModelSesionKey];

			// If the view model is null, redirect back to the create screen
			if (createGroupModel == null)
			{
				return new RedirectResult("/admin/groups/create");
			}



			return new RedirectResult("/admin/groups");
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