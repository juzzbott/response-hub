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

		/// <summary>
		/// Gets all the sign in entries for the specific unit
		/// </summary>
		/// <param name="unitId">The unit id to get the results for.</param>
		/// <returns>The list of sign in types for the unit.</returns>
		public async Task<IList<SignInEntry>> GetSignInsForUnit(Guid unitId, DateTime dateFrom, DateTime dateTo)
		{
			return await GetSignInsForUnit(unitId, dateFrom, dateTo, SignInType.Operation | SignInType.Training | SignInType.Other);
		}

		/// <summary>
		/// Gets the sign in entries for the specific unit, based on the type of sign in types.
		/// </summary>
		/// <param name="unitId">The unit id to get the results for.</param>
		/// <param name="signInTypes">The sign in flag types to return.</param>
		/// <returns>The list of sign in types for the unit.</returns>
		public async Task<IList<SignInEntry>> GetSignInsForUnit(Guid unitId, DateTime dateFrom, DateTime dateTo, SignInType signInTypes)
		{
			return await _repository.GetSignInsForUnit(unitId, dateFrom, dateTo, signInTypes);
		}

		/// <summary>
		/// Gets the sign in entries for the specific job ids.
		/// </summary>
		/// <param name="jobMessageIds">The collection of job ids to get the sign in entries for.</param>
		/// <returns>The list of sign ins that match the job ids.</returns>
		public async Task<IList<SignInEntry>> GetSignInsForJobMessages(IEnumerable<Guid> jobMessageIds)
		{
			return await _repository.GetSignInsForJobMessages(jobMessageIds);
		}
		
		/// <summary>
		/// Gets the sign in entries for the specific job id.
		/// </summary>
		/// <param name="jobMessageId">The job id to get the sign in entries for.</param>
		/// <returns>The list of sign ins that match the job id.</returns>
		public async Task<IList<SignInEntry>> GetSignInsForJobMessage(Guid jobMessageId)
		{
			return await _repository.GetSignInsForJobMessages(new Guid[] { jobMessageId });
		}

		/// <summary>
		/// Counts how many sign in entries the user has where there is no sign out date recorded.
		/// </summary>
		/// <param name="userId">The id of the user to search for.</param>
		/// <returns>The number of sign in enties where there is no sign out date/time for the specified user.</returns>
		public async Task<int> CountSignOutsRequiredForUser(Guid userId)
		{
			return await _repository.CountSignOutsRequiredForUser(userId);
		}

		/// <summary>
		/// Gets sign in entries where the user has no sign out date recorded.
		/// </summary>
		/// <param name="userId">The id of the user to search for.</param>
		/// <returns>The list of sign in enties where there is no sign out date/time for the specified user.</returns>
		public async Task<IList<SignInEntry>> GetSignInsWithoutSignOutsForUser(Guid userId)
		{
			return await _repository.GetSignInsWithoutSignOutsForUser(userId);
		}
	}
}
