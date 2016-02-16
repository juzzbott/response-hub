using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{
	[MongoCollectionName("pager_messages")]
	public class PagerMessageRepository : MongoRepository<PagerMessage>, IPagerMessageRepository
	{
	}
}
