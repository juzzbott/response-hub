﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Spatial;
using Enivate.ResponseHub.Model.Spatial;

using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{

	[MongoCollectionName("map_indexes")]
	public class MapIndexRepository : MongoRepository<MapIndexDto>, IMapIndexRepository
	{

		/// <summary>
		/// The logger interface
		/// </summary>
		ILogger _log;

		#region Constructor

		public MapIndexRepository(ILogger log)
		{
			_log = log;
		}

		public MapIndexRepository(ILogger log, string connectionString) : base(connectionString)
		{
			_log = log;
		}

		#endregion;

		/// <summary>
		/// Batch inserts the map indexes into the database.
		/// </summary>
		/// <param name="mapIndexes">The collection of map indexes to insert.</param>
		/// <returns></returns>
		public async Task BatchInsert(IList<MapIndex> mapIndexes)
		{

			// Create the list of map index dtos
			IList<MapIndexDto> mapIndexDtos = new List<MapIndexDto>();
			foreach(MapIndex mapIndex in mapIndexes)
			{
				mapIndexDtos.Add(MapModelToDataObject(mapIndex));
			}

			// Batch insert into the collection
			await Collection.InsertManyAsync(mapIndexDtos);
			
		}

		/// <summary>
		/// Clears the collection of map indexes.
		/// </summary>
		/// <returns></returns>
		public async Task ClearCollection()
		{
			await Collection.DeleteManyAsync(new BsonDocument());
		}

		/// <summary>
		/// Gets the map index based on the page number.
		/// </summary>
		/// <param name="mapPage">The page number of the map index to find.</param>
		/// <returns></returns>
		public async Task<MapIndex> GetMapIndexByPageNumber(MapType mapType, string mapPage)
		{

			// Get the map index from the db and return it.
			MapIndexDto mapIndex = await FindOne(i => i.MapType == mapType && i.PageNumber.ToLower() == mapPage.ToLower());
			return MapDataObjectToModel(mapIndex);
		}

		#region Model mapping

		/// <summary>
		/// Map the data object to the model map index.
		/// </summary>
		/// <param name="dataObject">The data object to map.</param>
		/// <returns>The data object.</returns>
		public MapIndex MapDataObjectToModel(MapIndexDto dataObject)
		{

			// Create the map index
			MapIndex mapIndex = new MapIndex()
			{
				Id = dataObject.Id,
				MapType = dataObject.MapType,
				PageNumber = dataObject.PageNumber,
				Scale = dataObject.Scale,
				UtmNumber = dataObject.UtmNumber
			};

			// Map the grid references
			foreach(GridReferenceDto gridRefDto in dataObject.GridReferences)
			{
				// Create the GridReference
				GridReference gridRef = new GridReference()
				{
					GridSquare = gridRefDto.GridSquare,
					Latitude = gridRefDto.Coordinates.Latitude,
					Longitude = gridRefDto.Coordinates.Longitude
				};
				mapIndex.GridReferences.Add(gridRef);
			}

			// return the map index
			return mapIndex;

		}

		/// <summary>
		/// Map the model object to the data map index.
		/// </summary>
		/// <param name="modelObject">The model object to map.</param>
		/// <returns>The data object.</returns>
		public MapIndexDto MapModelToDataObject(MapIndex modelObject)
		{

			// Create the map index
			MapIndexDto mapIndex = new MapIndexDto()
			{
				Id = modelObject.Id,
				MapType = modelObject.MapType,
				PageNumber = modelObject.PageNumber,
				Scale = modelObject.Scale,
				UtmNumber = modelObject.UtmNumber
			};

			// Map the grid references
			foreach (GridReference gridRef in modelObject.GridReferences)
			{

				if (gridRef == null)
				{
					int i = 0;
				}

				if (gridRef.GridSquare == null)
				{
					int i = 0;
				}

				if (gridRef.Longitude == null)
				{
					int i = 0;
				}

				if (gridRef.Latitude == null)
				{
					int i = 0;
				}

				if (mapIndex == null)
				{
					int i = 0;
				}

				if (mapIndex.GridReferences == null)
				{
					int i = 0;
				}

				// Create the GridReference
				GridReferenceDto gridRefDto = new GridReferenceDto()
				{
					GridSquare = gridRef.GridSquare,
					Coordinates = new GeoJson2DGeographicCoordinates(gridRef.Longitude, gridRef.Latitude)
				};
				mapIndex.GridReferences.Add(gridRefDto);
			}

			// return the map index
			return mapIndex;

		}

		#endregion

	}
}
