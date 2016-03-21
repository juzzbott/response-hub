using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Agencies;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{

	[MongoCollectionName("agencies")]
	public class AgencyRepository : MongoRepository<Agency>, IAgencyRepository
	{
	}
}
