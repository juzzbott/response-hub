using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.SignIn.Interface
{
	public interface ISignInEntryService
	{

		Task SignUserIn(SignInEntry signOn);

		Task SignUserOut(Guid signOnId, DateTime signOutTime);

		Task<IList<SignInEntry>> GetSignInsForUser(Guid userId);

		Task<IList<SignInEntry>> GetSignInsForGroup(Guid groupId, DateTime dateFrom, DateTime dateTo);

		Task<IList<SignInEntry>> GetSignInsForGroup(Guid groupId, DateTime dateFrom, DateTime dateTo, SignInType signInTypes);

	}
}
