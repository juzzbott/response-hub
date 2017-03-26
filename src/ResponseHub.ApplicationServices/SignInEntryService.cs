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

		/// <summary>
		/// Signs the user out for the particular sign on session.
		/// </summary>
		/// <param name="signOnId">The id of the sign in entry.</param>
		/// <param name="signOutTime">The datetime the user signed out.</param>
		/// <returns></returns>
		public async Task SignUserOut(Guid signOnId, DateTime signOutTime)
		{
			await _repository.SignUserOut(signOnId, signOutTime);
		}

		/// <summary>
		/// Creates a new sign in entry for the user.
		/// </summary>
		/// <param name="signOn">The sign on entry to store in the database.</param>
		/// <returns></returns>
		public async Task SignUserIn(SignInEntry signOn)
		{
			await _repository.SignUserIn(signOn);
		}

		/// <summary>
		/// Gets the sign in history for the specified user.
		/// </summary>
		/// <param name="userId">The ID of the user to get the sign ins for.</param>
		/// <returns>The list of sign in entries for the user.</returns>
		public async Task<IList<SignInEntry>> GetSignInsForUser(Guid userId)
		{
			return await _repository.GetSignInsForUser(userId);
		}
	}
}
