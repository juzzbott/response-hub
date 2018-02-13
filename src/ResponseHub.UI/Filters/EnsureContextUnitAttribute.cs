using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.Model.Units.Interface;

namespace Enivate.ResponseHub.UI.Filters
{
	public class EnsureContextUnitAttribute : ActionFilterAttribute, IActionFilter
	{
		protected IUnitService UnitService = ServiceLocator.Get<IUnitService>();

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{

			if (filterContext != null && filterContext.HttpContext != null)
			{

				Guid userId;

				if (filterContext.HttpContext.User.Identity.IsAuthenticated)
				{
					userId = new Guid(filterContext.HttpContext.User.Identity.GetUserId());
				}
				else
				{
					return;
				}


				if (filterContext.HttpContext.Session[SessionConstants.ContextUnitId] == null)
				{
					// Get the unit ids that the user is a unit administrator of
					IList<Guid> unitIds = Task.Run(async () => await UnitService.GetUnitIdsUserIsUnitAdminOf(userId)).Result;

					// If there is 1 unit, add the context unit id to the session
					if (unitIds.Count == 1)
					{
						Guid unitId = unitIds.First();
						filterContext.HttpContext.Session[SessionConstants.ContextUnitId] = unitId;
					}

				}

			}
		}
	}
}