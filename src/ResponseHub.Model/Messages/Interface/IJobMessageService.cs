using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Groups;

namespace Enivate.ResponseHub.Model.Messages.Interface
{
	public interface IJobMessageService
	{

		Task AddMessages(IList<JobMessage> messages);

		Task<IList<JobMessage>> GetMostRecent(IEnumerable<Capcode> capcodes, MessageType messageTypes, int count, int skip);

		Task<IList<JobMessage>> GetMostRecent(int count, int skip);

		Task<IList<JobMessage>> GetMessagesBetweenDates(IEnumerable<Capcode> capcodes, MessageType messageTypes, int count, int skip, DateTime? dateFrom, DateTime? dateTo);

		Task<IList<JobMessage>> GetMostRecent(Guid lastId);

		Task<IList<JobMessage>> GetLatestFromLastMessage(Guid lastId, IEnumerable<string> capcodes, MessageType messageTypes);

		Task<JobMessage> GetById(Guid id);

		Task<JobMessage> GetByJobNumber(string jobNumber);

		Task<JobNote> AddNoteToJobMessage(Guid jobMessageId, string noteBody, bool isWordBack, Guid userId);

		Task<IList<JobNote>> GetNotesForJob(Guid jobMessageId);

		Task<MessageProgress> SaveProgress(Guid jobMessageId, DateTime progressDateTime, Guid userId, MessageProgressType progressType);

		Task RemoveProgress(Guid jobMessageId, MessageProgressType progressType);

		Task<PagedResultSet<JobMessage>> FindByKeyword(string keyword, IEnumerable<string> capcodes, MessageType messageTypes, DateTime dateFrom, DateTime dateTo, int limit, int skip, bool countTotal);

		Task AddAttachmentToJob(Guid jobMessageId, Guid attachmentId);

		Task RemoveAttachmentFromJob(Guid jobMessageId, Guid attachmentId);
	}
}
