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
		
	}
}
