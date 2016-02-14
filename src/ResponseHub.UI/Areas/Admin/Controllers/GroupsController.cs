using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;

using Enivate.ResponseHub.ApplicationServices.Identity;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;

using Enivate.ResponseHub.UI.Areas.Admin.Models.Groups;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.UI.Models.Users;

namespace Enivate.ResponseHub.UI.Areas.Admin.Controllers
{

	[RouteArea("admin")]
	[RoutePrefix("groups")]
	[ClaimsAuthorize(Roles = RoleTypes.SystemAdministrator)]
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

		private ILogger _log;
		protected ILogger Log
		{
			get
			{
				return _log ?? (_log = UnityConfiguration.Container.Resolve<ILogger>());
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

			// If the model is not valid, return view.
			if (!ModelState.IsValid)
			{
				return View(model);
			}

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

			ViewBag.FormAction = "/admin/groups/create/group-administrator";

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
				model.GroupAdministrator.UserExists = true;
			}

			return View("AssignUser", model.GroupAdministrator);
		}

		[Route("create/group-administrator")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<ActionResult> GroupAdministrator(GroupAdministratorViewModel model)
		{

			ViewBag.FormAction = "/admin/groups/create/group-administrator";

			// If the model is not valid, return view.
			if (!ModelState.IsValid)
			{
				return View("AssignUser", model);
			}

			// Get the CreateGroupViewModel from session
			CreateGroupModel createGroupModel = (CreateGroupModel)Session[CreateGroupViewModelSesionKey];

			// If the view model is null, redirect back to the create screen
			if (createGroupModel == null)
			{
				return new RedirectResult("/admin/groups/create");
			}

			// Store the group administrator
			Guid groupAdministratorId;

			// If the group administrator user does not exist, then create the user now
			if (!model.UserExists)
			{

				// Create the new user
				IdentityUser newUser = await UserService.CreateAsync(model.EmailAddress, model.FirstName, model.Surname, new List<string>() { RoleTypes.GroupAdministrator });

				// Set the group administrator to the new user id
				groupAdministratorId = newUser.Id;

			}
			else
			{
				// Get the identity user related to the specified group admin
				IdentityUser groupAdminUser = await UserService.FindByEmailAsync(createGroupModel.GroupAdministratorEmail);

				// If the group admin user is null, return an error, otherwise set the group admin id.
				if (groupAdminUser == null)
				{
					ModelState.AddModelError("", "There was a system error creating the group.");
					await Log.Error(String.Format("Unable to create group. Existing user with email ''  could not be found.", createGroupModel.GroupAdministratorEmail));
					return View("AssignUser", model);
				}
				else
				{
					groupAdministratorId = groupAdminUser.Id;
				}
			}

			// Get the service type from the model
			int groupServiceId;
			Int32.TryParse(createGroupModel.Service, out groupServiceId);
			ServiceType service = (ServiceType)groupServiceId;

			// Create the group
			await GroupService.CreateGroup(createGroupModel.Name, service, createGroupModel.Capcode, groupAdministratorId, createGroupModel.Description);

			// redirect back to group index page
			return new RedirectResult("/admin/groups?group_created=1");
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

			// Get the list of users in the group from the Users property of the group
			IList<IdentityUser> groupUsers = await UserService.GetUsersByIds(group.Users.Select(i => i.UserId));

			// Create the list of GroupUserViewModels for the users in the group
			IList<GroupUserViewModel> groupUserModels = new List<GroupUserViewModel>();
			foreach(IdentityUser groupUser in groupUsers)
			{
				groupUserModels.Add(new GroupUserViewModel() {
					EmailAddress = groupUser.EmailAddress,
					FirstName = groupUser.FirstName,
					GroupRole = group.Users.FirstOrDefault(i => i.UserId == groupUser.Id).Role,
					Id = groupUser.Id,
					Surname = groupUser.Surname
				});
			}

			// Create the model for the single view
			SingleGroupViewModel model = new SingleGroupViewModel()
			{
				Id = group.Id,
				Name = group.Name,
				Description = group.Description,
				Service = EnumValue.GetEnumDescription(group.Service),
				Capcode = group.Capcode,
				Users = groupUserModels
			};

			return View(model);


		}

	}
}