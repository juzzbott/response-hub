using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver.GeoJsonObjectModel;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Messages;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Spatial;
using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{

	[MongoCollectionName("job_messages")]
	public class JobMessageRepository : MongoRepository<JobMessageDto>, IJobMessageRepository
	{

		/// <summary>
		/// Adds the messages to the database.
		/// </summary>
		/// <param name="messages"></param>
		/// <returns></returns>
		public async Task AddMessages(IList<JobMessage> messages)
		{

			// Write the job messages to the database.
			await Collection.InsertManyAsync(messages.Select(i => MapModelToDbObject(i)));
		}

		#region Mappers

		/// <summary>
		/// Maps the JobMessageDto data object to the JobMessage model object.
		/// </summary>
		/// <param name="dbObject"></param>
		/// <returns></returns>
		public JobMessage MapDbObjectToModel(JobMessageDto dbObject)
		{
			JobMessage model = new JobMessage()
			{
				Capcode = dbObject.Capcode,
				Id = dbObject.Id,
				JobNumber = dbObject.JobNumber,
				MessageContent = dbObject.MessageContent,
				Priority = dbObject.Priority,
				Timestamp = dbObject.Timestamp
			};

			// Map the location property.
			if (dbObject.Location != null)
			{
				model.Location = MapLocationInfoDbObjectToModel(dbObject.Location);
			}
			else
			{
				model.Location = new LocationInfo();
			}

			// return the model
			return model;
		}

		/// <summary>
		/// Maps the JobMessage model object to the JobMessageDto data object.
		/// </summary>
		/// <param name="modelObject"></param>
		/// <returns></returns>
		public JobMessageDto MapModelToDbObject(JobMessage modelObject)
		{
			JobMessageDto dbObject = new JobMessageDto()
			{
				Capcode = modelObject.Capcode,
				Id = modelObject.Id,
				JobNumber = modelObject.JobNumber,
				MessageContent = modelObject.MessageContent,
				Priority = modelObject.Priority,
				Timestamp = modelObject.Timestamp
			};

			// Map the location property
			if (modelObject.Location != null)
			{
				dbObject.Location = MapLocationInfoModelToDbObject(modelObject.Location);
			} 
			else
			{
				dbObject.Location = new LocationInfoDto();
			}

			// return the db object
			return dbObject;
		}

		/// <summary>
		/// Maps the LocationInfoDto data object to the LocationInfo model object.
		/// </summary>
		/// <param name="dbObject"></param>
		/// <returns></returns>
		private LocationInfo MapLocationInfoDbObjectToModel(LocationInfoDto dbObject)
		{
			LocationInfo model = new LocationInfo()
			{
				AddressInfo = dbObject.AddressInfo,
				GridReference = dbObject.GridReference,
				MapPage = dbObject.MapPage,
				MapReference = dbObject.MapReference,
				MapType = dbObject.MapType
			};

			if (dbObject.Coordinates != null)
			{
				model.Coordinates = new Coordinates(dbObject.Coordinates.Latitude, dbObject.Coordinates.Longitude);
			}

			return model;
		}

		/// <summary>
		/// Maps the LocationInfo model object to the LocationInfoDto object.
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		private LocationInfoDto MapLocationInfoModelToDbObject(LocationInfo modelObject)
		{
			LocationInfoDto dbObject = new LocationInfoDto()
			{
				AddressInfo = modelObject.AddressInfo,
				GridReference = modelObject.GridReference,
				MapPage = modelObject.MapPage,
				MapReference = modelObject.MapReference,
				MapType = modelObject.MapType
			};

			// Set the coordinates field if not null
			if (modelObject.Coordinates != null)
			{
				dbObject.Coordinates = new GeoJson2DGeographicCoordinates(modelObject.Coordinates.Longitude, modelObject.Coordinates.Latitude);
			}

			// return the db object
			return dbObject;
		}

		#endregion

	}
}
