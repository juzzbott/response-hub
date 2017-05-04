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

		Task AddSignIn(SignInEntry signOn);

		Task AddSignIns(IEnumerable<SignInEntry> signIns);

		Task SignUserOut(Guid signOnId, DateTime signOutTime);

		Task<IList<SignInEntry>> GetSignInsForUser(Guid userId);

		Task<IList<SignInEntry>> GetSignInsForUnit(Guid unitId, DateTime from, DateTime to, SignInType signInTypes);

		Task<IList<SignInEntry>> GetSignInsForJobMessages(IEnumerable<Guid> jobMessageIds);

		Task<int> CountSignOutsRequiredForUser(Guid userId);

		Task<IList<SignInEntry>> GetSignInsWithoutSignOutsForUser(Guid userId);

	}
}
