using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.SignIn;
using Enivate.ResponseHub.Model.SignIn.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class SignInEntryService : ISignInEntryService
	{

		private ISignInEntryRepository _repository;

		public SignInEntryService(ISignInEntryRepository repository)
		{
			_repository = repository;
		}

		public async Task SignUserOut(Guid signOnId, DateTime signOutTime)
		{
			await _repository.SignUserOut(signOnId, signOutTime);
		}

		public async Task SignUserIn(SignInEntry signOn)
		{
			await _repository.SignUserIn(signOn);
		}
	}
}
