using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Groups;

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

	}
}
