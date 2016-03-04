using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface IJobMessageRepository
	{

		Task AddMessages(IList<JobMessage> messages);

	}
}
