using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.SignOn.Interface
{
	public interface ISignOnEntryService
	{

		Task SignUserIn(SignOnEntry signOn);

		Task SignUserOut(Guid signOnId, DateTime signOutTime);

	}
}
