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

		Task<IList<JobMessage>> GetMostRecent(IEnumerable<string> capcodes, int count, MessageType messageTypes);

		Task<JobMessage> GetById(Guid id);

		Task AddNoteToJobMessage(Guid jobMessageId, JobNote note);

		Task AddProgress(Guid jobMessageId, MessageProgress progress);

	}
}
