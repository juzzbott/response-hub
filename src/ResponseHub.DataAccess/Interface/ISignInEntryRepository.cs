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

	}
}
