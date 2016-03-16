using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Messages.Interface
{
	public interface IJobMessageService
	{

		Task AddMessages(IList<JobMessage> messages);

		Task<IList<JobMessage>> GetMostRecent(IEnumerable<string> capcodes, int count);

		Task<JobMessage> GetById(Guid id);

		Task<JobNote> AddNoteToJobMessage(Guid jobMessageId, string noteBody, bool isWordBack, Guid userId);

		Task<MessageProgress> AddProgress(Guid jobMessageId, Guid userId, MessageProgressType progressType);
	}
}
