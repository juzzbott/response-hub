using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.UI.Filters;

namespace Enivate.ResponseHub.UI.Controllers.Api
{

    [RoutePrefix("api/units")]
    public class UnitsController : BaseApiController
    {

        private IUnitService UnitService;

        public UnitsController()
        {
            UnitService = ServiceLocator.Get<IUnitService>();
        }


        [Route("{unitId:guid}")]
        [HttpGet]
        [OverrideActionFilters]
        [OverrideAuthorization]
        [AllowAnonymous]
        public async Task<Unit> GetById(Guid unitId)
        {

            // Validate the APiKey header
            if (await ValidateApiKeyHeader() == false)
            {
                return null;
            }

            return await UnitService.GetById(unitId);
        }

    }
}
