using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Areas.Admin.Models.Units;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.Common.CsvParser;
using System.Data;
using System.Text;
using Enivate.ResponseHub.Model.Units;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{

	[RouteArea("ControlPanel", AreaPrefix ="control-panel")]
	[RoutePrefix("units")]
	[ClaimsAuthorize(Roles = RoleTypes.UnitAdministrator)]
	public class UnitsController : BaseControlPanelController
    {

		[Route]
        // GET: ControlPanel/Units
        public ActionResult Index()
        {
			Guid unitId = GetControlPanelUnitId();
			return new RedirectResult(String.Format("/control-panel/units/{0}", unitId));
		}
		
		#region View unit

		[Route("{id:guid}")]
		[HttpGet]
		public async Task<ActionResult> ViewUnit(Guid id)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(id) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await GetViewUnitViewResult(id, "~/Areas/ControlPanel/Views/Units/ViewUnit.cshtml");
		}

		#endregion

		#region Edit unit

		[Route("{id:guid}/edit")]
		[HttpGet]
		public async Task<ActionResult> Edit(Guid id)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(id) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await GetEditUnitViewResult(id, "~/Areas/ControlPanel/Views/Units/Edit.cshtml");
		}

		[Route("{id:guid}/edit")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<ActionResult> Edit(Guid id, CreateUnitModel model)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(id) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await PostEditUnitViewResult(id, model, "~/Areas/ControlPanel/Views/Units/Edit.cshtml", false);
		}

		#endregion
		
		#region New user

		[Route("{unitId:guid}/add-member")]
		[HttpGet]
		public async Task<ActionResult> AddMember(Guid unitId)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(unitId) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await GetAddMemberViewResult(unitId);
		}


		[Route("{unitId:guid}/add-member")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> AddMember(Guid unitId, NewUserViewModel model)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(unitId) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await PostAddMemberViewResult(unitId, model);
		}

		[Route("{unitId:guid}/confirm-member")]
		[HttpGet]
		public async Task<ActionResult> ConfirmMember(Guid unitId)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(unitId) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await GetConfirmMemberViewResult(unitId, "~/Areas/ControlPanel/Views/Units/ConfirmUser.cshtml");
		}

		[Route("{unitId:guid}/confirm-member")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ConfirmMember(Guid unitId, ConfirmUserViewModel model)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(unitId) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await PostConfirmMemberViewResult(unitId, model, "~/Areas/ControlPanel/Views/Units/ConfirmUser.cshtml");
		}

        #endregion

        #region Bulk Add Members

        [Route("{unitId:guid}/bulk-add-members")]
        [HttpGet]
        public async Task<ActionResult> BulkAddMembers(Guid unitId)
        {

            BulkMemberUploadViewModel model = new BulkMemberUploadViewModel()
            {
                UnitId = unitId
            };

            // If the current user is not a unit admin of the specified unit, error out.
            if (await CurrentUserIsAdminOfUnit(unitId) == false)
            {
                throw new HttpException(403, "The user does not have access to this url.");
            }

            return View("~/Areas/ControlPanel/Views/Units/BulkAddMembers.cshtml", model);
        }

        [Route("{unitId:guid}/bulk-add-members/sample-csv-file")]
        [HttpGet]
        public ActionResult SampleCsvFile(Guid unitId)
        {

            StringBuilder sbFileContents = new StringBuilder();
            sbFileContents.AppendLine("Email Address,First Name,Surname,Member Number,Group Access Level");
            sbFileContents.AppendLine("john.smith@members.ses.vic.gov.au,John,Smith,12345,Unit administrator");
            sbFileContents.AppendLine("jane.doe@members.ses.vic.gov.au,Jane,Doe,12346,General user");

            return File(Encoding.UTF8.GetBytes(sbFileContents.ToString()), "text/csv", "sample-bulk-member.csv");
        }

        [Route("{unitId:guid}/bulk-add-members")]
        [HttpPost]
        public async Task<ActionResult> BulkAddMembers(Guid unitId, BulkMemberUploadViewModel model)
        {

            // If the current user is not a unit admin of the specified unit, error out.
            if (await CurrentUserIsAdminOfUnit(unitId) == false)
            {
                throw new HttpException(403, "The user does not have access to this url.");
            }

            model.UnitId = unitId;
            DataTable parsedFile;

            // Parse the CSV file

            try
            {
                CsvParser parser = new CsvParser(model.File.InputStream, true);
                parsedFile = parser.ReadCsvDataTable();
            }
            catch (Exception ex)
            {
                await Log.Error("Unable to parse CSV file", ex);
                model.InvalidDataFileColumns = true;
                return View("~/Areas/ControlPanel/Views/Units/BulkAddMembers.cshtml", model);
            }

            // If there are not 5 columns, file is invalid so return error message
            if (parsedFile.Columns.Count != 5)
            {
                model.InvalidDataFileColumns = true;
                return View("~/Areas/ControlPanel/Views/Units/BulkAddMembers.cshtml", model);
            }

            IList<BulkMemberUploadItemViewModel> newMembersFromFile = ParseBulkMemberItems(parsedFile);

            // Set the file upload to true so we know to display summary
            model.FileUploaded = true;
            model.TotalMembersInFile = newMembersFromFile.Count;

            // Get the current unit
            Unit unit = await UnitService.GetById(unitId);

            // Loop through each item and determine which list it goes in
            foreach (BulkMemberUploadItemViewModel bulkMemberItem in newMembersFromFile)
            {
                string errorMessage = bulkMemberItem.IsValid();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    model.InvalidUsers.Add(new KeyValuePair<BulkMemberUploadItemViewModel, string>(bulkMemberItem, errorMessage));
                    continue;
                }

                // If there is a duplicate, then add an error 
                if (newMembersFromFile.Count(i => i.EmailAddress.Equals(bulkMemberItem.EmailAddress, StringComparison.CurrentCultureIgnoreCase)) > 1)
                {
                    model.InvalidUsers.Add(new KeyValuePair<BulkMemberUploadItemViewModel, string>(bulkMemberItem, String.Format("Email address '{0}' duplicated in file.", bulkMemberItem.EmailAddress)));
                    continue;
                }

                // Get an existing identity user by email if it exists
                IdentityUser existingUser = await UserService.GetByEmailAddress(bulkMemberItem.EmailAddress);
                if (existingUser != null)
                {

                    // Check if any members already exist in the current unit
                    if (unit.Users.Any(i => i.UserId == existingUser.Id))
                    {
                        model.ExistingUnitMembers.Add(bulkMemberItem);
                    }
                    else
                    {
                        model.MembersToBeAddedToUnit.Add(bulkMemberItem);
                    }

                    // We can't create a user that already exists
                    continue;
                }

                // Add to the valid users list
                model.UsersToBeImported.Add(bulkMemberItem);
            }

            // Add the model to session to be used for the actual import
            Session.Add(SessionConstants.BulkMemberUploadModel, model);

            return View("~/Areas/ControlPanel/Views/Units/BulkAddMembers.cshtml", model);
        }

        [Route("{unitId:guid}/bulk-add-members/import-members")]
        [HttpGet]
        public async Task<ActionResult> ImportMembers(Guid unitId)
        {

            // If the current user is not a unit admin of the specified unit, error out.
            if (await CurrentUserIsAdminOfUnit(unitId) == false)
            {
                throw new HttpException(403, "The user does not have access to this url.");
            }

            // Get the model from the session
            BulkMemberUploadViewModel importModel = (BulkMemberUploadViewModel)Session[SessionConstants.BulkMemberUploadModel];

            // If the model is null, something went wrong with session, so redirect back
            if (importModel == null)
            {
                return new RedirectResult(string.Format("{0}?invalid_session=1", Request.Url.PathAndQuery.ToLower().Replace("/import-members", "")));
            }

            BulkMemberImportedViewModel model = new BulkMemberImportedViewModel()
            {
                UnitId = unitId
            };
            
            // Loop through the users to be created
            foreach(BulkMemberUploadItemViewModel item in importModel.UsersToBeImported)
            {
                try
                {
                    // Create the profile
                    UserProfile profile = new UserProfile()
                    {
                        MemberNumber = item.MemberNumber
                    };

                    // Get the roles for the imported user
                    List<string> roles = GetUserRoleForImportedUser(item);

                    // Create the new user, and then create the unit mapping for the new user.
                    IdentityUser newUser = await UserService.CreateAsync(item.EmailAddress, item.FirstName, item.Surname, roles, profile, false);

                    // Now that we have the newUser, create the user mapping.
                    await UnitService.AddUserToUnit(newUser.Id, roles[0], unitId);

                    // Update the model data
                    model.ImportedMembers.Add(item);
                    model.TotalMembersProcessed++;
                }
                catch (Exception ex)
                {
                    string errorMessage = string.Format("Error bulk importing member '{0}'.", item.EmailAddress);
                    await Log.Error(errorMessage, ex);
                    model.ErrorMembers.Add(new KeyValuePair<BulkMemberUploadItemViewModel, string>(item, errorMessage));
                }
            }

            // Loop through the existing users to be added to the unit
            foreach (BulkMemberUploadItemViewModel item in importModel.MembersToBeAddedToUnit)
            {
                try
                {
                    // Get the identity user related to the specified unit admin
                    IdentityUser newUser = await UserService.FindByEmailAsync(item.EmailAddress);

                    // Get the roles for the imported user
                    List<string> roles = GetUserRoleForImportedUser(item);

                    // Create the user mapping for the existing user
                    await UnitService.AddUserToUnit(newUser.Id, roles[0], unitId);

                    // Update the model data
                    model.AddedToUnit.Add(item);
                    model.TotalMembersProcessed++;
                }
                catch (Exception ex)
                {
                    string errorMessage = string.Format("Error bulk adding member '{0}' to unit .", item.EmailAddress);
                    await Log.Error(errorMessage, ex);
                    model.ErrorMembers.Add(new KeyValuePair<BulkMemberUploadItemViewModel, string>(item, errorMessage));
                }
            }

            return View("~/Areas/ControlPanel/Views/Units/BulkMembersImported.cshtml", model);
        }

        private static List<string> GetUserRoleForImportedUser(BulkMemberUploadItemViewModel item)
        {
            string role = (item.UserAccessType.Equals("Unit administrator", StringComparison.CurrentCultureIgnoreCase) ? RoleTypes.UnitAdministrator : RoleTypes.GeneralUser);

            // Create the list of roles
            List<string> roles = new List<string>() { role };

            // If there is no "General User" role, add it so that they can access the basic site areas
            if (!roles.Contains(RoleTypes.GeneralUser))
            {
                roles.Add(RoleTypes.GeneralUser);
            }

            return roles;
        }

        /// <summary>
        /// Parse the data table into a list of bulk member items
        /// </summary>
        /// <param name="parsedFile"></param>
        /// <returns></returns>
        private IList<BulkMemberUploadItemViewModel> ParseBulkMemberItems(DataTable parsedFile)
        {
            IList<BulkMemberUploadItemViewModel> memberItems = new List<BulkMemberUploadItemViewModel>();

            // Loop through each data row in the table
            foreach (DataRow row in parsedFile.Rows)
            {
                BulkMemberUploadItemViewModel memberItem = new BulkMemberUploadItemViewModel()
                {
                    EmailAddress = (string)row[0],
                    FirstName = (string)row[1],
                    Surname = (string)row[2],
                    MemberNumber = (string)row[3],
                    UserAccessType = (string)row[4]
                };
                memberItems.Add(memberItem);
            }

            // return the member items
            return memberItems;
        }

        #endregion

        #region Remove User From Unit

        [Route("{unitId:guid}/remove-user/{userId:guid}")]
		public async Task<ActionResult> RemoveUserFromUnit(Guid unitId, Guid userId)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(unitId) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await GetRemoveUserFromUnit(unitId, userId, "control-panel");
		}

		#endregion

		#region Change User Role

		[Route("{unitId:guid}/change-role/{userId:guid}")]
		[HttpGet]
		public async Task<ActionResult> ChangeUserRole(Guid unitId, Guid userId)
		{

			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(unitId) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return GetChangeUserRoleViewResult(unitId, userId, "~/Areas/ControlPanel/Views/Units/ChangeUserRole.cshtml");
		}

		[Route("{unitId:guid}/change-role/{userId:guid}")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangeUserRole(Guid unitId, Guid userId, ChangeUserRoleViewModel model)
		{
			
			// If the current user is not a unit admin of the specified unit, error out.
			if (await CurrentUserIsAdminOfUnit(unitId) == false)
			{
				throw new HttpException(403, "The user does not have access to this url.");
			}

			return await ViewChangeUserRoleViewResult(unitId, userId, model, "~/Areas/ControlPanel/Views/Units/ChangeUserRole.cshtml", "control-panel");
		}

		#endregion

	}
}