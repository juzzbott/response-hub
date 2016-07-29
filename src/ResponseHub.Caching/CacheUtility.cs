using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.Caching
{
	public static class CacheUtility
	{


		/// <summary>
		/// Gets the cache key for the entity object.
		/// </summary>
		/// <param name="entity">The IEtnity object to generate the cache key for.</param>
		/// <returns>The cache key for the entity.</returns>
		public static string GetEntityCacheKey(IEntity entity)
		{
			return GetEntityCacheKey(entity.GetType(), entity.Id.ToString());
		}

		/// <summary>
		/// Gets the cache key for the entity object.
		/// </summary>
		/// <param name="entityType">The IEtnity object to generate the cache key for.</param>
		/// <param name="id">The ID of the entity to get the cache item for.</param>
		/// <returns>The cache key for the entity.</returns>
		public static string GetEntityCacheKey(Type entityType, string id)
		{
			// Get the name of the type
			string typeName = entityType.Name;

			// Create the cache key based on the type name and id
			string key = String.Format("{0}_{1}", typeName, id);
			return key;
		}

	}
}
