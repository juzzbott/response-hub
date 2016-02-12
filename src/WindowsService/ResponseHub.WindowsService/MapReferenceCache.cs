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
		public IDictionary<string, IList<MapIndex>> SpatialVision { get; set; }

		/// <summary>
		/// The collection of Melway map references.
		/// </summary>
		public IDictionary<string, IList<MapIndex>> Melway { get; set; }

		/// <summary>
		/// Creates a new instance of the MapReferenceCache.
		/// </summary>
		private MapReferenceCache()
		{
			SpatialVision = new Dictionary<string, IList<MapIndex>>();
			Melway = new Dictionary<string, IList<MapIndex>>();
		}

		public void AddMapReference(MapType mapType, MapIndex mapReference)
		{

			if (mapType == MapType.Unknown)
			{
				return;
			}

			IDictionary<string, IList<MapIndex>> collection;

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
				collection.Add(mapReference.PageNumber, new List<MapIndex>());
			}

			// Now we can add the map reference to the list of references for the specific page
			collection[mapReference.PageNumber].Add(mapReference);

		}

		public bool CacheItemExists(MapType mapType, MapIndex mapReference)
		{
			if (mapType == MapType.Unknown || mapReference == null)
			{
				return false;
			}

			throw new NotImplementedException();
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
