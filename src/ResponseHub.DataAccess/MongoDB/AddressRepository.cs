using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Addresses;
using Enivate.ResponseHub.Model.Addresses;

using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{

	[MongoCollectionName("addresses")]
	public class AddressRepository : MongoRepository<StructuredAddressDto>, IAddressRepository
	{

		/// <summary>
		/// Gets the structured address based on the address query hash.
		/// </summary>
		/// <param name="addressQueryHash">The address query hash to search by.</param>
		/// <returns>The structured address object if found, otherwise return null.</returns>
		public async Task<StructuredAddress> GetByAddressQueryHash(string addressQueryHash)
		{

			// Build the query
			FilterDefinition<StructuredAddressDto> filter = Builders<StructuredAddressDto>.Filter.Eq(i => i.AddressQueryHash, addressQueryHash);

			// Return the structured address that has the hash if one exists
			StructuredAddressDto result = await Collection.Find(filter).FirstOrDefaultAsync();

			return MapDbObjectToModel(result);

		}
		

		/// <summary>
		/// Gets an address based on the id.
		/// </summary>
		/// <param name="id">The Id of the address to get.</param>
		/// <returns>The address if found, otherwise null.</returns>
		public new async Task<StructuredAddress> GetById(Guid id)
		{
			// Get the data object from the db
			StructuredAddressDto address = await Collection.Find(Builders<StructuredAddressDto>.Filter.Eq(i => i.Id, id)).SingleOrDefaultAsync();

			// return the mapped job message
			return MapDbObjectToModel(address);
		}

		/// <summary>
		/// Adds a new structured address to the database.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public async Task<StructuredAddress> Add(StructuredAddress address)
		{
			await Collection.InsertOneAsync(MapModelObjectToDb(address));
			return address;
		}

		#region Mappers

		/// <summary>
		/// Maps the database dto object to the model object.
		/// </summary>
		/// <param name="dbObject"></param>
		/// <returns></returns>
		public StructuredAddress MapDbObjectToModel(StructuredAddressDto dbObject)
		{

			// If null, return a null object.
			if (dbObject == null)
			{
				return null;
			}

			// Return a new model object
			return new StructuredAddress()
			{
				AddressQueryHash = dbObject.AddressQueryHash,
				GoogleGeocodeId = dbObject.GoogleGeocodeId,
				Id = dbObject.Id,
				Latitude = dbObject.Coordinates.Latitude,
				Longitude = dbObject.Coordinates.Longitude,
				Postcode = dbObject.Postcode,
				State = dbObject.State,
				Street = dbObject.Street,
				StreetNumber = dbObject.StreetNumber,
				Suburb = dbObject.Suburb,
				Unit = dbObject.Unit
			};
		}

		/// <summary>
		/// Maps the model object to the database dto object.
		/// </summary>
		/// <param name="modelObject"></param>
		/// <returns></returns>
		public StructuredAddressDto MapModelObjectToDb(StructuredAddress modelObject)
		{

			// If the model is null, return a null dto
			if (modelObject == null)
			{
				return null;
			}

			// return a new address dto
			return new StructuredAddressDto()
			{
				AddressQueryHash = modelObject.AddressQueryHash,
				GoogleGeocodeId = modelObject.GoogleGeocodeId,
				Id = modelObject.Id,
				Coordinates = new GeoJson2DGeographicCoordinates(modelObject.Longitude, modelObject.Latitude),
				Postcode = modelObject.Postcode,
				State = modelObject.State,
				Street = modelObject.Street,
				StreetNumber = modelObject.StreetNumber,
				Suburb = modelObject.Suburb,
				Unit = modelObject.Unit
			};
		}

		#endregion

	}
}
