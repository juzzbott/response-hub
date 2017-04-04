using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.SignIn;

using MongoDB.Driver;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{
	[MongoCollectionName("user_sign_ins")]
	public class SignInEntryRepository : MongoRepository<SignInEntry>, ISignInEntryRepository
	{

		/// <summary>
		/// Signs the user out for the particular sign on session.
		/// </summary>
		/// <param name="signOnId">The id of the sign in entry.</param>
		/// <param name="signOutTime">The datetime the user signed out.</param>
		/// <returns></returns>
		public async Task SignUserOut(Guid signOnId, DateTime signOutTime)
		{
			// Get the filter based on id
			FilterDefinition<SignInEntry> filter = Builders<SignInEntry>.Filter.Eq(i => i.Id, signOnId);

			// Set the update
			UpdateDefinition<SignInEntry> update = Builders<SignInEntry>.Update.Set(i => i.SignOutTime, signOutTime);

			// Perform the update
			await Collection.UpdateOneAsync(filter, update);
		}

		/// <summary>
		/// Creates a new sign in entry for the user.
		/// </summary>
		/// <param name="signOn">The sign on entry to store in the database.</param>
		/// <returns></returns>
		public async Task SignUserIn(SignInEntry signOn)
		{
			await Add(signOn);
		}

		/// <summary>
		/// Gets the sign in history for the specified user.
		/// </summary>
		/// <param name="userId">The ID of the user to get the sign ins for.</param>
		/// <returns>The list of sign in entries for the user.</returns>
		public async Task<IList<SignInEntry>> GetSignInsForUser(Guid userId)
		{
			// Create the filter definition
			FilterDefinition<SignInEntry> filter = Builders<SignInEntry>.Filter.Eq(i => i.UserId, userId);

			// Create the sort definition
			SortDefinition<SignInEntry> sort = Builders<SignInEntry>.Sort.Descending(i => i.SignInTime).Descending(i => i.Created);

			// Get the sign ins for the user
			IList<SignInEntry> signIns = await Collection.Find(filter).Sort(sort).ToListAsync();

			// return the list of mapped sign ins
			return signIns;

		}

		/// <summary>
		/// Gets the sign in entries for the specific group, based on the type of sign in types.
		/// </summary>
		/// <param name="groupId">The group id to get the results for.</param>
		/// <param name="signInTypes">The sign in flag types to return.</param>
		/// <returns>The list of sign in types for the group.</returns>
		public async Task<IList<SignInEntry>> GetSignInsForGroup(Guid groupId, DateTime from, DateTime to, SignInType signInTypes)
		{
			
			// Create the filter
			FilterDefinitionBuilder<SignInEntry> builder = Builders<SignInEntry>.Filter;
			FilterDefinition<SignInEntry> filter = builder.Eq(i => i.GroupId, groupId);

			// Set the date range
			filter = filter & builder.Gte(i => i.SignInTime, from) & builder.Lte(i => i.SignInTime, to);

			// Set the message types
			filter = filter & builder.BitsAnySet(i => i.SignInType, (long)signInTypes);

			// return the sign in entries
			IList<SignInEntry> signIns = await Collection.Find(filter).ToListAsync();

			// Map the results to model objects
			return signIns;


		}

		/// <summary>
		/// Gets the sign in entries for the specific job id.
		/// </summary>
		/// <param name="jobMessageId">The job id to get the sign in entries for.</param>
		/// <returns>The list of sign ins that match the job id.</returns>
		public async Task<IList<SignInEntry>> GetSignInsForJobMessages(IEnumerable<Guid> jobMessageIds)
		{
			// Create the filter for operation sign ins and the job id
			FilterDefinition<SignInEntry> filter = Builders<SignInEntry>.Filter.Eq(i => i.SignInType, SignInType.Operation) & Builders<SignInEntry>.Filter.In(i => i.OperationDetails.JobId, jobMessageIds);

			// return the results
			return await Collection.Find(filter).ToListAsync();
		}

		/// <summary>
		/// Counts how many sign in entries the user has where there is no sign out date recorded.
		/// </summary>
		/// <param name="userId">The id of the user to search for.</param>
		/// <returns>The number of sign in enties where there is no sign out date/time for the specified user.</returns>
		public async Task<int> CountSignOutsRequiredForUser(Guid userId)
		{
			// Create the filter
			FilterDefinition<SignInEntry> filter = Builders<SignInEntry>.Filter.Eq(i => i.UserId, userId) & Builders<SignInEntry>.Filter.Eq(i => i.SignOutTime, null);

			// return the count of matched documents
			long results = await Collection.CountAsync(filter);
			return (int)results;
		}

		/// <summary>
		/// Gets sign in entries where the user has no sign out date recorded.
		/// </summary>
		/// <param name="userId">The id of the user to search for.</param>
		/// <returns>The list of sign in enties where there is no sign out date/time for the specified user.</returns>
		public async Task<IList<SignInEntry>> GetSignInsWithoutSignOutsForUser(Guid userId)
		{
			// Create the filter
			FilterDefinition<SignInEntry> filter = Builders<SignInEntry>.Filter.Eq(i => i.UserId, userId) & Builders<SignInEntry>.Filter.Eq(i => i.SignOutTime, null);

			// return the list of sign ins
			return await Collection.Find(filter).ToListAsync();
		}
	}
}
