using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{
	[MongoCollectionName("pager_messages")]
	public class PagerMessageRepository : MongoRepository<PagerMessage>, IPagerMessageRepository
	{
	}
}
