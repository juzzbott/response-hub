using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.UI.Controllers;
using System.Security.Claims;
using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{
    public abstract class BaseControlPanelController : BaseController
    {
        public IList<Guid> GetGroupIdsUserIsGroupAdmin()
		{
			// Get the user mappings for groups for the user
			return new List<Guid>();

		}
    }
}