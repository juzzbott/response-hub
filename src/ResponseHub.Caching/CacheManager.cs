using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.Caching
{
    public static class CacheManager
    {

		private const string CacheName = "ResponseHub_Cache";

		private static volatile MemoryCache _instance;
		private static volatile ConcurrentDictionary<string, DateTime> _keysInstance = new ConcurrentDictionary<string, DateTime>();
		private static object _lock = new Object();
		private static object _keysLock = new Object();

		public static MemoryCache Cache
		{
			get
			{
				// Perform a double check lock on the private instance to implement singleton
				if (_instance == null)
				{
					lock (_lock)
					{
						if (_instance == null)
						{
							_instance = new MemoryCache(CacheName);
						}
					}
				}
				// return the cache instance.
				return _instance;
			}
		}

		private static ConcurrentDictionary<string, DateTime> Keys
		{
			get
			{
				// Perform a double check lock on the private instance to implement singleton
				if (_keysInstance == null)
				{
					lock (_keysLock)
					{
						if (_keysInstance == null)
						{
							_keysInstance = new ConcurrentDictionary<string, DateTime>();
						}
					}
				}
				// return the cache instance.
				return _keysInstance;
			}
		}

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
			if (entity == null || entity.Id == Guid.Empty)
			{
				return;
			}

			// Get the cache key for the entity
			string key = CacheUtility.GetEntityCacheKey(entity);

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
				AbsoluteExpiration = expiry,
				RemovedCallback = CacheRemovedCallback
			};

			// Add to the memory cache
			Cache.Add(cacheItem, policy);

			// If the cache key already exists, remove it
			if (Keys.ContainsKey(key))
			{
                Keys.TryRemove(key, out DateTime dt);
            }

			// Add the cache key to the lookup keys.
			Keys.TryAdd(key, policy.AbsoluteExpiration.DateTime);

		}

		#endregion

		#region Get Item

		/// <summary>
		/// Gets an entity from the cache.
		/// </summary>
		/// <typeparam name="T">The type of the entity.</typeparam>
		/// <param name="id">The ID of the entity to get from the cache.</param>
		/// <returns>The entity obejct if found or null if expired or non-existant.</returns>
		public static T GetEntity<T>(Guid id) where T : IEntity
		{
			// If the id is null or empty, return
			if (id == Guid.Empty)
			{
				return default(T);
			}

			// Get the cache key based on the type and id.
			string key = CacheUtility.GetEntityCacheKey(typeof(T), id.ToString());

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
			T item = (T)Cache.Get(key);

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
			string key = CacheUtility.GetEntityCacheKey(entity);

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
			Cache.Remove(key);
		}
		
		#endregion

		#region Clear Items

		public static void ClearCache()
		{
			lock (_lock)
			{
				_instance = new MemoryCache(CacheName);
				_keysInstance = new ConcurrentDictionary<string, DateTime>();
			}
		}

		#endregion

		#region Cache sizes and item information

		public static long GetItemCount()
		{
			return Cache.GetCount();
		}

		public static IList<string> GetCacheKeys()
		{
			// If the keys is null, return empty list
			if (Keys == null)
			{
				return new List<string>();
			}

			// Return the list of keys
			return Keys.Select(i => i.Key).ToList();
		}

		public static ConcurrentDictionary<string, DateTime> GetCacheKeysWithExpiry()
		{
			return Keys;
		}

		public static long CacheMemoryLimit()
		{
			return Cache.CacheMemoryLimit;
		}

		public static long CachePhysicalMemoryLimit()
		{
			return Cache.PhysicalMemoryLimit;
		}

		public static TimeSpan PollingInterval()
		{
			return Cache.PollingInterval;
		}

		#endregion

		#region Remove item callback
		private static void CacheRemovedCallback(CacheEntryRemovedArguments args)
		{
            Keys.TryRemove(args.CacheItem.Key, out DateTime dt);
        }
		#endregion

	}
}
