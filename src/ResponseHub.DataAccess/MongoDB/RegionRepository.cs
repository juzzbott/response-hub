using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{

	[MongoCollectionName("regions")]
	public class RegionRepository : MongoRepository<Region>, IRegionRepository
	{

		/// <summary>
		/// The ILogger that is responsible for logging data.
		/// </summary>
		private ILogger _logger;

		public RegionRepository(ILogger logger)
		{
			_logger = logger;
		}

	}
}
