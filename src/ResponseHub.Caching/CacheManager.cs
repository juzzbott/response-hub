using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.Caching
{
    public class CacheManager
    {

		private const int DefaultTimeoutMinutes = 60;

		#region Add Item

		/// <summary>
		/// Adds a mongo entity to the cache.
		/// </summary>
		/// <param name="item">The item to add to the cache.</param>
		public static void AddItem(IEntity entity)
		{
			AddItem(entity, DateTime.Now.AddMinutes(DefaultTimeoutMinutes));
		}

		/// <summary>
		/// Adds a mongo entity to the cache.
		/// </summary>
		/// <param name="entity">The IEntity object to add to the cache.</param>
		/// <param name="timeToLive">The time, in minutes, that the object is valid in the cache for.</param>
		public static void AddItem(IEntity entity, int timeToLive)
		{
			AddItem(entity, DateTime.Now.AddMinutes(timeToLive));
		}

		/// <summary>
		/// Adds a mongo entity to the cache. Default expiry is 10 minutes.
		/// </summary>
		/// <param name="item">The item to add to the cache.</param>
		/// <param name="expiry">The date and time that the object should expire.</param>
		public static void AddItem(IEntity entity, DateTime expiry)
		{

			// If the item is null, then return
			if (entity == null)
			{
				return;
			}

			// Get the cache key for the entity
			string key = getEntityCacheKey(entity);

			// Add the entity to cache
			AddItem(key, entity, expiry);

		}


		/// <summary>
		/// Adds an item to the cache. Default expiry is 10 minutes.
		/// </summary>
		/// <param name="key">The key to store as the cache key.</param>
		/// <param name="item">The item to add to the cache.</param>
		public static void AddItem(string key, object item)
		{
			AddItem(key, item, DefaultTimeoutMinutes);
		}

		/// <summary>
		/// Adds an item to the cache. Default expiry is 10 minutes.
		/// </summary>
		/// <param name="key">The key to store as the cache key.</param>
		/// <param name="item">The item to add to the cache.</param>
		/// <param name="timeToLive">The time, in minutes, that the object is valid in the cache for.</param>
		public static void AddItem(string key, object item, int timeToLive)
		{
			AddItem(key, item, DateTime.Now.AddMinutes(timeToLive));
		}

		/// <summary>
		/// Adds an item to the cache.
		/// </summary>
		/// <param name="key">The key to store as the cache key.</param>
		/// <param name="item">The item to add to the cache.</param>
		/// <param name="expiry">The date and time that the object should expire.</param>
		public static void AddItem(string key, object item, DateTime expiry)
		{

			// If the item is null, then return
			if (item == null || String.IsNullOrEmpty(key))
			{
				return;
			}

			// Create the cache item
			CacheItem cacheItem = new CacheItem(key, item);

			// Create the cache policy
			CacheItemPolicy policy = new CacheItemPolicy()
			{
				AbsoluteExpiration = expiry
			};

			// Add to the memory cache
			MemoryCache.Default.Add(cacheItem, policy);

		}

		#endregion

		#region Get Item

		/// <summary>
		/// Gets an entity from the cache.
		/// </summary>
		/// <typeparam name="T">The type of the entity.</typeparam>
		/// <param name="id">The ID of the entity to get from the cache.</param>
		/// <returns>The entity obejct if found or null if expired or non-existant.</returns>
		public static T GetEntity<T>(string id) where T : IEntity
		{
			// If the id is null or empty, return
			if (String.IsNullOrEmpty(id))
			{
				return default(T);
			}

			// Get the cache key based on the type and id.
			string key = getEntityCacheKey(typeof(T), id);

			// Return the cache item.
			return GetItem<T>(key);
		}

		/// <summary>
		/// Gets the object from the cache as type T.
		/// </summary>
		/// <typeparam name="T">The type of object to return from the cache.</typeparam>
		/// <param name="key">The key of the cache item.</param>
		/// <returns>The cache item, or default(T) if not found.</returns>
		public static T GetItem<T>(string key)
		{

			// If the cache key is null or empty, return
			if (String.IsNullOrEmpty(key))
			{
				return default(T);
			}

			// Get the cache item
			T item = (T)MemoryCache.Default.Get(key);

			// return the cache item
			return item;

		}

		#endregion

		#region Remove Items

		/// <summary>
		/// Removes the specified entity object from the cache.
		/// </summary>
		/// <param name="entity"></param>
		public static void RemoveEntity(IEntity entity)
		{

			if (entity == null)
			{
				return;
			}

			// Get the cache key for the entity
			string key = getEntityCacheKey(entity);

			// Remvoe the entity from the cache.
			RemoveItem(key);
		}

		/// <summary>
		/// Removes the object with the specified ckey from the cache.
		/// </summary>
		/// <param name="key"></param>
		public static void RemoveItem(string key)
		{
			if (String.IsNullOrEmpty(key))
			{
				return;
			}

			// Remove the item from cache
			MemoryCache.Default.Remove(key);
		}

		#endregion

		#region Helpers 

		/// <summary>
		/// Gets the cache key for the entity object.
		/// </summary>
		/// <param name="entity">The IEtnity object to generate the cache key for.</param>
		/// <returns>The cache key for the entity.</returns>
		private static string getEntityCacheKey(IEntity entity)
		{
			return getEntityCacheKey(entity.GetType(), entity.Id.ToString());
		}

		/// <summary>
		/// Gets the cache key for the entity object.
		/// </summary>
		/// <param name="entityType">The IEtnity object to generate the cache key for.</param>
		/// <param name="id">The ID of the entity to get the cache item for.</param>
		/// <returns>The cache key for the entity.</returns>
		private static string getEntityCacheKey(Type entityType, string id)
		{
			// Get the name of the type
			string typeName = entityType.Name;

			// Create the cache key based on the type name and id
			string key = String.Format("{0}_{1}", typeName, id);
			return key;
		}

		#endregion

	}
}
