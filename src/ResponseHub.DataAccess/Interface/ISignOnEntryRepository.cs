using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.SignOn;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface ISignOnEntryRepository
	{

		Task SignUserIn(SignOnEntry signOn);

		Task SignUserOut(Guid signOnId, DateTime signOutTime);

	}
}
