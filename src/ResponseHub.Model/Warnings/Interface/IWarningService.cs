using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Warnings.Interface
{
	public interface IWarningService
	{

		IList<IWarning> GetWarnings(WarningSource source);

		IList<IWarning> GetBureauOfMeteorologyWarnings();

		IList<IWarning> GetStateEmergencyServiceWarnings();

		IList<IWarning> GetCountryFireAuthorityWarnings();

	}
}
