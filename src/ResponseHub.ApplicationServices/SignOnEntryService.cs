using Enivate.ResponseHub.DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.SignOn.Interface;
using Enivate.ResponseHub.Model.SignOn;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class SignOnEntryService : ISignOnEntryService
	{

		private ISignOnEntryRepository _repository;

		public SignOnEntryService(ISignOnEntryRepository repository)
		{
			_repository = repository;
		}

		public async Task SignUserOut(Guid signOnId, DateTime signOutTime)
		{
			await _repository.SignUserOut(signOnId, signOutTime);
		}

		public async Task SignUserIn(SignOnEntry signOn)
		{
			await _repository.SignUserIn(signOn);
		}
	}
}
