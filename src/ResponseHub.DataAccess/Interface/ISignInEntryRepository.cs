using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.SignIn;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface ISignInEntryRepository
	{

		Task SignUserIn(SignInEntry signOn);

		Task SignUserOut(Guid signOnId, DateTime signOutTime);

		Task<IList<SignInEntry>> GetSignInsForUser(Guid userId);

		Task<IList<SignInEntry>> GetSignInsForGroup(Guid groupId, DateTime from, DateTime to, SignInType signInTypes);

		Task<IList<SignInEntry>> GetSignInsForJobMessages(IEnumerable<Guid> jobMessageIds);

		Task<int> CountSignOutsRequiredForUser(Guid userId);

		Task<IList<SignInEntry>> GetSignInsWithoutSignOutsForUser(Guid userId);

	}
}
