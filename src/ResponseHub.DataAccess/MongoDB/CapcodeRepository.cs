using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Groups;

using MongoDB.Driver;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{

	[MongoCollectionName("capcodes")]
	public class CapcodeRepository : MongoRepository<Capcode>, ICapcodeRepository
	{
		
		/// <summary>
		/// Finds the capcodes that match the name entered.
		/// </summary>
		/// <param name="name">The name to find the capcode by.</param>
		/// <returns>The list of capcodes matching the result.</returns>
		public async Task<IList<Capcode>> FindByName(string name)
		{

			// Get the results of the text search.
			PagedResultSet<Capcode> results = await TextSearch(name, Int32.MaxValue, 0, false);
			
			// return the mapped groups.
			return results.Items;
		}
		
		/// <summary>
		/// Gets the list of capcodes that have ids in the collection of capcodeIds
		/// </summary>
		/// <param name="capcodeIds">The collection of capcode ids to search for.</param>
		/// <returns>The list of capcodes that have ids in the collection of ids.</returns>
		public async Task<IList<Capcode>> GetCapcodesById(IList<Guid> capcodeIds)
		{
			return await Collection.Find(Builders<Capcode>.Filter.In(i => i.Id, capcodeIds)).ToListAsync();
		}

		/// <summary>
		/// Gets the list of full capcode objects based on the list of capcode address values.
		/// </summary>
		/// <param name="capcodes">List of capcode address values.</param>
		/// <returns>The list of capcode objects.</returns>
		public async Task<IList<Capcode>> GetCapcodes(IList<string> capcodes)
		{
			return await Collection.Find(Builders<Capcode>.Filter.In(i => i.CapcodeAddress, capcodes)).ToListAsync();
		}

		/// <summary>
		/// Gets the capcode from the capcode address in the database.
		/// </summary>
		/// <param name="capcodeAddress"></param>
		/// <returns></returns>
		public async Task<Capcode> GetByCapcodeAddress(string capcodeAddress)
		{
			return await Collection.Find(Builders<Capcode>.Filter.Eq(i => i.CapcodeAddress, capcodeAddress)).FirstOrDefaultAsync();
		}

		/// <summary>
		/// Gets the capcodes that are not specified as Group capcodes.
		/// </summary>
		/// <returns>The list of capcodes where IsGroupCapcode is false.</returns>
		public async Task<IList<Capcode>> GetSharedCapcodes()
		{
			return await Collection.Find(Builders<Capcode>.Filter.Eq(i => i.IsGroupCapcode, false)).ToListAsync();
		}

		/// <summary>
		/// Gets the capcodes that are only specified for use as Group capcodes or not based on the groupOnly parameter.
		/// </summary>
		/// <returns>The list of capcodes where IsGroupCapcode is false.</returns>
		public async Task<IList<Capcode>> GetAllByGroupOnly(bool groupOnly)
		{
			return await Collection.Find(Builders<Capcode>.Filter.Eq(i => i.IsGroupCapcode, groupOnly)).ToListAsync();
		}

	}
}
