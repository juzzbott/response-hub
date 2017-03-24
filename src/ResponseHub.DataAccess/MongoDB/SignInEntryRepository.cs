using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.SignIn;

using MongoDB.Driver;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.SignIn;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{
	[MongoCollectionName("user_sign_ins")]
	public class SignInEntryRepository : MongoRepository<SignInEntryDto>, ISignInEntryRepository
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
			FilterDefinition<SignInEntryDto> filter = Builders<SignInEntryDto>.Filter.Eq(i => i.Id, signOnId);

			// Set the update
			UpdateDefinition<SignInEntryDto> update = Builders<SignInEntryDto>.Update.Set(i => i.SignOutTime, signOutTime);

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
			await Add(MapModelObjectToDb(signOn));
		}

		/// <summary>
		/// Gets the sign in history for the specified user.
		/// </summary>
		/// <param name="userId">The ID of the user to get the sign ins for.</param>
		/// <returns>The list of sign in entries for the user.</returns>
		public async Task<IList<SignInEntry>> GetSignInsForUser(Guid userId)
		{
			// Create the filter definition
			FilterDefinition<SignInEntryDto> filter = Builders<SignInEntryDto>.Filter.Eq(i => i.UserId, userId);

			// Create the sort definition
			SortDefinition<SignInEntryDto> sort = Builders<SignInEntryDto>.Sort.Descending(i => i.SignInTime).Descending(i => i.Created);

			// Get the sign ins for the user
			IList<SignInEntryDto> signIns = await Collection.Find(filter).Sort(sort).ToListAsync();

			// return the list of mapped sign ins
			return signIns.Select(i => MapDbObjectToModel(i)).ToList();

		}

		#region Mappers

		/// <summary>
		/// Maps the SignOnEntry data object to the model object.
		/// </summary>
		/// <param name="dbObject">The sign on entry db object</param>
		/// <returns>The mapped SignOnEntry model object.</returns>
		private SignInEntry MapDbObjectToModel(SignInEntryDto dbObject)
		{
			// if db object is null, return null
			if (dbObject == null)
			{
				return null;
			}

			// return the mapped object
			return new SignInEntry()
			{
				ActivityDetails = MapActivityDbObjectToModel(dbObject.ActivityDetails),
				Created = dbObject.Created,
				GroupId = dbObject.GroupId,
				Id = dbObject.Id,
				SignInTime = dbObject.SignInTime,
				SignInType = dbObject.SignInType,
				SignOutTime = dbObject.SignOutTime,
				UserId = dbObject.UserId
			};
		}

		/// <summary>
		/// Maps the SignOnEntry data object to the model object.
		/// </summary>
		/// <param name="modelObject">The sign on entry db object</param>
		/// <returns>The mapped SignOnEntry model object.</returns>
		private SignInEntryDto MapModelObjectToDb(SignInEntry modelObject)
		{
			// if db object is null, return null
			if (modelObject == null)
			{
				return null;
			}

			// return the mapped object
			return new SignInEntryDto()
			{
				ActivityDetails = MapActivityModelToDbObject(modelObject.ActivityDetails),
				Created = modelObject.Created,
				GroupId = modelObject.GroupId,
				Id = modelObject.Id,
				SignInTime = modelObject.SignInTime,
				SignInType = modelObject.SignInType,
				SignOutTime = modelObject.SignOutTime,
				UserId = modelObject.UserId
			};
		}

		/// <summary>
		/// Maps the activity model object to the data object.
		/// </summary>
		/// <param name="activityDetails">The model activity object.</param>
		/// <returns>The mapped activity data object.</returns>
		private ActivityDto MapActivityModelToDbObject(Activity activityDetails)
		{
			// If the activity details are null, return null
			if (activityDetails == null)
			{
				return null;
			}

			// If it's an operation activity, map that
			if (activityDetails.GetType() == typeof(OperationActivity))
			{
				// Cast to correct type
				OperationActivity operationActivity = (OperationActivity)activityDetails;

				// Return typed operation activity
				return new OperationActivityDto()
				{
					Description = operationActivity.Description,
					JobId = operationActivity.JobId
				};
			}
			// Otherwise, if it's training activity, map that
			else if (activityDetails.GetType() == typeof(TrainingActivity))
			{
				// Cast to correct type
				TrainingActivity trainingActivity = (TrainingActivity)activityDetails;

				// Return typed operation activity
				return new TrainingActivityDto()
				{
					OtherDescription = trainingActivity.OtherDescription,
					TrainingType = trainingActivity.TrainingType
				};
			}

			// Other type, so just return null
			return null;
		}

		/// <summary>
		/// Maps the activity database object to the model object.
		/// </summary>
		/// <param name="activityDetails">The database activity object.</param>
		/// <returns>The mapped activity model object.</returns>
		private Activity MapActivityDbObjectToModel(ActivityDto activityDetails)
		{
			// If the activity details are null, return null
			if (activityDetails == null)
			{
				return null;
			}

			// If it's an operation activity, map that
			if (activityDetails.GetType() == typeof(OperationActivityDto))
			{
				// Cast to correct type
				OperationActivityDto operationActivity = (OperationActivityDto)activityDetails;

				// Return typed operation activity
				return new OperationActivity()
				{
					Description = operationActivity.Description,
					JobId = operationActivity.JobId
				};
			}
			// Otherwise, if it's training activity, map that
			else if (activityDetails.GetType() == typeof(TrainingActivityDto))
			{
				// Cast to correct type
				TrainingActivityDto trainingActivity = (TrainingActivityDto)activityDetails;

				// Return typed operation activity
				return new TrainingActivity()
				{
					OtherDescription = trainingActivity.OtherDescription,
					TrainingType = trainingActivity.TrainingType
				};
			}

			// Other type, so just return null
			return null;
		}

		#endregion

	}
}
