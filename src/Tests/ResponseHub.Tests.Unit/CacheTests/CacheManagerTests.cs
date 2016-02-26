using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Caching;

using Xunit;
using System.Threading;

namespace Enivate.ResponseHub.Tests.Unit.CacheTests
{
	public class CacheManagerTests
	{

		FakeEntity _fakeEntity;
		string _fakeObject;
		string _cacheKeyFakeObject = "fake_object";

		public CacheManagerTests()
		{

			_fakeEntity = new FakeEntity()
			{
				Id = Guid.NewGuid(),
				Name = "Fake Entity",
				OtherData = ""
			};

			_fakeObject = "Some Object Data";

			// Add the entity
			CacheManager.AddItem(_fakeEntity);
			CacheManager.AddItem(_cacheKeyFakeObject, _fakeObject);

		}

		[Fact(DisplayName = "Can Get Entity Item From Cache")]
		[Trait("Category", "Cache Manager")]
		public void Can_Get_Entity_Item_From_Cache()
		{

			// Get from the cache
			FakeEntity fromCache = CacheManager.GetEntity<FakeEntity>(_fakeEntity.Id);

			// Ensure it exists and there is a name
			Assert.NotNull(fromCache);
			Assert.True(!String.IsNullOrEmpty(fromCache.Name));

		}

		[Fact(DisplayName = "Can Get Object Item From Cache")]
		[Trait("Category", "Cache Manager")]
		public void Can_Get_Object_Item_From_Cache()
		{

			// Get from the cache
			string fromCache = CacheManager.GetItem<string>(_cacheKeyFakeObject);

			// Ensure it exists and there is a name
			Assert.NotNull(fromCache);
			Assert.True(!String.IsNullOrEmpty(fromCache));

		}

		[Fact(DisplayName = "Ensure Expired Object Not Returned From Cache")]
		[Trait("Category", "Cache Manager")]
		public void Ensure_Expired_Object_Not_Returned_From_Cache()
		{

			FakeEntity expiredObject = new FakeEntity()
			{
				Id = Guid.NewGuid(),
				Name = "Something Expired"
			};

			CacheManager.AddItem(expiredObject, DateTime.Now.AddMilliseconds(500));

			// Ensure it exists and there is a name
			FakeEntity fromCache = CacheManager.GetEntity<FakeEntity>(expiredObject.Id);
			Assert.NotNull(fromCache);
			Assert.True(!String.IsNullOrEmpty(fromCache.Name));

			Thread.Sleep((int)TimeSpan.FromSeconds(3).TotalMilliseconds);

			// Ensure it no longer exists
			FakeEntity fromCacheExpired = CacheManager.GetEntity<FakeEntity>(expiredObject.Id);
			Assert.Null(fromCacheExpired);

		}

		[Fact(DisplayName = "Can Remove Object From Cache")]
		[Trait("Category", "Cache Manager")]
		public void Can_Remove_Object_From_Cache()
		{

			FakeEntity toRemoveObject = new FakeEntity()
			{
				Id = Guid.NewGuid(),
				Name = "Something To Remove"
			};

			// Add to the cache
			CacheManager.AddItem(toRemoveObject);

			// Ensure it exists and there is a name
			FakeEntity fromCache = CacheManager.GetEntity<FakeEntity>(toRemoveObject.Id);
			Assert.NotNull(fromCache);
			Assert.True(!String.IsNullOrEmpty(fromCache.Name));

			// Remove from the cache
			CacheManager.RemoveEntity(toRemoveObject);

			// Ensure it no longer exists
			FakeEntity fromCacheRemoved = CacheManager.GetEntity<FakeEntity>(toRemoveObject.Id);
			Assert.Null(fromCacheRemoved);

		}

	}
}
