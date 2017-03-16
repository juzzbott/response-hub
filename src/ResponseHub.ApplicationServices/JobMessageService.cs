using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Groups;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class JobMessageService : IJobMessageService
	{

		private ILogger _log;
		private IJobMessageRepository _repository;

		/// <summary>
		/// Creates a new instance of the ILogger log writer
		/// </summary>
		/// <param name="log"></param>
		public JobMessageService(IJobMessageRepository repository, ILogger log)
		{
			_log = log;
			_repository = repository;
		}

		/// <summary>
		/// Adds the job messages to the collection.
		/// </summary>
		/// <param name="messages"></param>
		/// <returns></returns>
		public async Task AddMessages(IList<JobMessage> messages)
		{
			// If the messages is null, then return null.
			if (messages == null)
			{
				await _log.Warn("The 'messages' parameter was null.");
			}

			// If the collection is empty, just return an empty string
			if (messages.Count == 0)
			{
				return;
			}

			// Save the messages to the repository
			await _repository.AddMessages(messages);
			
		}

		/// <summary>
		/// Gets the most recent job messages, limited by count and skip.
		/// </summary>
		/// <param name="count"></param>
		/// <param name="skip"></param>
		/// <returns></returns>
		public async Task<IList<JobMessage>> GetMostRecent(int count, int skip)
		{
			return await _repository.GetMostRecent(count, skip);
		}

		/// <summary>
		/// Gets the most recent job messages, up to the last message id.
		/// </summary>
		/// <param name="count"></param>
		/// <param name="skip"></param>
		/// <returns></returns>
		public async Task<IList<JobMessage>> GetMostRecent(Guid lastId)
		{
			return await _repository.GetMostRecent(lastId);
		}

		/// <summary>
		/// Gets the most recent count capcodes. 
		/// </summary>
		/// <param name="capcodes"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public async Task<IList<JobMessage>> GetMostRecent(IEnumerable<Capcode> capcodes, MessageType messageTypes, int count)
		{
			return await _repository.GetMostRecent(capcodes.Select(i => i.CapcodeAddress), messageTypes, count);
		}

		/// <summary>
		/// Gets the list of latest messages that are new since the last message. The results are limited to the selected message types and capcodes.
		/// </summary>
		/// <param name="lastId"></param>
		/// <param name="capcodes"></param>
		/// <param name="messageTypes"></param>
		/// <returns></returns>
		public async Task<IList<JobMessage>> GetLatestFromLastMessage(Guid lastId, IEnumerable<string> capcodes, MessageType messageTypes)
		{
			return await _repository.GetLatestFromLastMessage(lastId, capcodes, messageTypes);
		}

		/// <summary>
		/// Gets the specific job message by the Id.
		/// </summary>
		/// <param name="id">The Id of the job to return.</param>
		/// <returns>The job message if found, otherwise null.</returns>
		public async Task<JobMessage> GetById(Guid id)
		{
			JobMessage message = await _repository.GetById(id);

			// Ensure the notes always come newest first.
			message.Notes = message.Notes.OrderByDescending(i => i.Created).ToList();

			// return the message
			return message;
		}

		/// <summary>
		/// Adds a new note to an existing job message. 
		/// </summary>
		/// <param name="jobMessageId">The id of the job message to add the note to.</param>
		/// <param name="note">The note to add to the job.</param>
		public async Task<JobNote> AddNoteToJobMessage(Guid jobMessageId, string noteBody, bool isWordBack, Guid userId)
		{

			// Create the job note
			JobNote note = new JobNote()
			{
				Body = noteBody,
				Created = DateTime.UtcNow,
				IsWordBack = isWordBack,
				UserId = userId
			};

			await _repository.AddNoteToJobMessage(jobMessageId, note);

			return note;
		}

		/// <summary>
		/// Gets the notes for specific job.
		/// </summary>
		/// <param name="jobMessageId">The ID of the job to get the notes for.</param>
		/// <returns>The job notes collection.</returns>
		public async Task<IList<JobNote>> GetNotesForJob(Guid jobMessageId)
		{
			return await _repository.GetNotesForJob(jobMessageId);
		}

		/// <summary>
		/// Adds the job progress to the job with the specified id.
		/// </summary>
		/// <param name="jobMessageId">The id of the job message to add the progress to.</param>
		/// <param name="userId">The id of the user who created the progress update.</param>
		/// <param name="progressType">The type of job progress to add,</param>
		/// <returns></returns>
		public async Task<MessageProgress> AddProgress(Guid jobMessageId, Guid userId, MessageProgressType progressType)
		{

			// Get the job
			JobMessage job = await GetById(jobMessageId);

			// If the job is already cancelled, throw error indicating job is already cancelled
			if (job.ProgressUpdates.Any(i => i.ProgressType == MessageProgressType.Cancelled))
			{
				throw new ApplicationException("Cannot add progress to cancelled job.");
			}

			// Create the progress object
			MessageProgress progress = new MessageProgress()
			{
				ProgressType = progressType,
				Timestamp = DateTime.UtcNow,
				UserId = userId
			};

			// Update the progress in the repository
			await _repository.AddProgress(jobMessageId, progress);

			// return the progress.
			return progress;

		}

		public async Task<PagedResultSet<JobMessage>> FindByKeyword(string keyword, IEnumerable<string> capcodes, MessageType messageTypes, DateTime dateFrom, DateTime dateTo, int limit, int skip, bool countTotal)
		{
			return await _repository.FindByKeyword(keyword, capcodes, messageTypes, dateFrom, dateTo, limit, skip, countTotal);
		}

		public async Task AddAttachmentToJob(Guid jobMessageId, Guid attachmentId)
		{
			await _repository.AddAttachmentToJob(jobMessageId, attachmentId);
		}

	}
}
