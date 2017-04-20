using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface IJobMessageRepository
	{

		Task AddMessages(IList<JobMessage> messages);

		Task<IList<JobMessage>> GetMostRecent(IEnumerable<string> capcodes, MessageType messageTypes, int count, int skip);

		Task<IList<JobMessage>> GetMostRecent(int count, int skip);

		Task<IList<JobMessage>> GetMessagesBetweenDates(IEnumerable<string> capcodes, MessageType messageTypes, int count, int skip, DateTime? dateFrom, DateTime? dateTo);

		Task<IList<JobMessage>> GetMostRecent(Guid lastId);

		Task<IList<JobMessage>> GetLatestFromLastMessage(Guid lastId, IEnumerable<string> capcodes, MessageType messageTypes);

		Task<JobMessage> GetById(Guid id);

		Task<IList<JobMessage>> GetByIds(IEnumerable<Guid> ids);

		Task<JobMessage> GetByJobNumber(string jobNumber);

		Task AddNoteToJobMessage(Guid jobMessageId, JobNote note);

		Task<IList<JobNote>> GetNotesForJob(Guid jobMessageId);

		Task SaveProgress(Guid jobMessageId, MessageProgress progress);

		Task RemoveProgress(Guid jobMessageId, MessageProgressType progressType);

		Task<PagedResultSet<JobMessage>> FindByKeyword(string keyword, IEnumerable<string> capcodes, MessageType messageTypes, DateTime dateFrom, DateTime dateTo, int limit, int skip, bool countTotal);

		Task AddAttachmentToJob(Guid jobMessageId, Guid attachmentId);

		Task RemoveAttachmentFromJob(Guid jobMessageId, Guid attachmentId);

	}
}
