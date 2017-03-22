﻿using System;
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

		Task<IList<JobMessage>> GetMostRecent(IEnumerable<string> capcodes, MessageType messageTypes, int count);

		Task<IList<JobMessage>> GetMostRecent(int count, int skip);

		Task<IList<JobMessage>> GetMostRecent(Guid lastId);

		Task<IList<JobMessage>> GetLatestFromLastMessage(Guid lastId, IEnumerable<string> capcodes, MessageType messageTypes);

		Task<JobMessage> GetById(Guid id);

		Task AddNoteToJobMessage(Guid jobMessageId, JobNote note);

		Task<IList<JobNote>> GetNotesForJob(Guid jobMessageId);

		Task AddProgress(Guid jobMessageId, MessageProgress progress);
		
		Task<IList<JobMessage>> GetJobMessagesBetweenDates(IEnumerable<string> capcodes, MessageType messageTypes, DateTime dateFrom, DateTime dateTo);

		Task<PagedResultSet<JobMessage>> FindByKeyword(string keyword, IEnumerable<string> capcodes, MessageType messageTypes, DateTime dateFrom, DateTime dateTo, int limit, int skip, bool countTotal);

		Task AddAttachmentToJob(Guid jobMessageId, Guid attachmentId);

	}
}
