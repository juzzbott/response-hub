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

		Task<IList<JobMessage>> GetMostRecent(IEnumerable<string> capcodes, MessageType messageTypes, int count);

		Task<IList<JobMessage>> GetLatestFromLastMessage(Guid lastId, IEnumerable<string> capcodes, MessageType messageTypes);

		Task<JobMessage> GetById(Guid id);

		Task<JobNote> AddNoteToJobMessage(Guid jobMessageId, string noteBody, bool isWordBack, Guid userId);

		Task<MessageProgress> AddProgress(Guid jobMessageId, Guid userId, MessageProgressType progressType);

		Task<IList<JobMessage>> GetJobMessagesBetweenDates(IEnumerable<string> capcodes, MessageType messageTypes, DateTime dateFrom, DateTime dateTo);
	}
}
