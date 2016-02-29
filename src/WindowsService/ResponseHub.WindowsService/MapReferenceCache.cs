using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.WindowsService
{
	public class MapReferenceCache
	{

		private static MapReferenceCache _instance;
		private static object _syncLock = new Object();

		/// <summary>
		/// The collection of Spatial Vision map references.
		/// </summary>
		public IDictionary<string, MapIndex> SpatialVision { get; set; }

		/// <summary>
		/// The collection of Melway map references.
		/// </summary>
		public IDictionary<string, MapIndex> Melway { get; set; }

		/// <summary>
		/// Creates a new instance of the MapReferenceCache.
		/// </summary>
		private MapReferenceCache()
		{
			SpatialVision = new Dictionary<string, MapIndex>();
			Melway = new Dictionary<string, MapIndex>();
		}

		public void AddMapReference(MapType mapType, MapIndex mapReference)
		{

			if (mapType == MapType.Unknown)
			{
				return;
			}

			IDictionary<string, MapIndex> collection;

			switch (mapType)
			{
				case MapType.SpatialVision:
					collection = SpatialVision;
					break;

				case MapType.Melway:
					collection = Melway;
					break;

				default:
					// If no map type is known, just return
					return;
			}

			// Now that we have the collection, first we need to check if the map page exists
			if (!collection.ContainsKey(mapReference.PageNumber))
			{
				collection.Add(mapReference.PageNumber, new MapIndex());
			}

			// Now we can add the map reference to the list of references for the specific page
			collection[mapReference.PageNumber] = mapReference;

		}

		/// <summary>
		/// Gets the cache item from memory.
		/// </summary>
		/// <param name="mapType">The map type to check for.</param>
		/// <param name="pageNumber">The page number for the map reference object.</param>
		/// <returns>True if the map reference exists in the cache, otherwise false.</returns>
		public MapIndex GetCacheItem(MapType mapType, string pageNumber)
		{
			if (mapType == MapType.Unknown || String.IsNullOrEmpty(pageNumber))
			{
				return null;
			}

			switch (mapType)
			{
				case MapType.SpatialVision:
					if (SpatialVision.ContainsKey(pageNumber))
					{
						return SpatialVision[pageNumber];
					}
					break;

				case MapType.Melway:
					if (Melway.ContainsKey(pageNumber))
					{
						return Melway[pageNumber];
					}
					break;
					
			}

			// If no map type is known, just return
			return null;

		}

		/// <summary>
		/// Static method to ensure the MapReferenceCache return the singleton instance.
		/// </summary>
		/// <returns>The singleton MapReferenceCache.</returns>
		public static MapReferenceCache Instance
		{
			get
			{
				// Initial check of null
				if (_instance == null)
				{
					// Lock the syncLock object to a single thread.
					lock(_syncLock)
					{
						// Double check the instance is null
						if (_instance == null)
						{
							// Instantiate the instance.
							_instance = new MapReferenceCache();
						}
					}
				}
				return _instance;
			}
		}

	}
}
