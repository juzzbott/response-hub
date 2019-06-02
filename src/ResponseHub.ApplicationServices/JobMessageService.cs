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
using Enivate.ResponseHub.Model.SignIn;
using Enivate.ResponseHub.Model.Attachments;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class JobMessageService : IJobMessageService
	{

		private ILogger _log;
		private IJobMessageRepository _repository;
        private ISignInEntryRepository _signInRepository;
        private IAttachmentRepository _attachmentRepository;

        /// <summary>
        /// Creates a new instance of the ILogger log writer
        /// </summary>
        /// <param name="log"></param>
        public JobMessageService(IJobMessageRepository repository, ISignInEntryRepository signInRepository, IAttachmentRepository attachmentRepository, ILogger log)
		{
			_log = log;
			_repository = repository;
            _signInRepository = signInRepository;
            _attachmentRepository = attachmentRepository;
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
        /// Gets the most recent count capcodes. 
        /// </summary>
        /// <param name="capcodes"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<IList<JobMessage>> GetMostRecent(IEnumerable<string> capcodes, int count, int skip)
        {
            return await _repository.GetMostRecent(capcodes, count, skip);
        }

        /// <summary>
        /// Gets the most recent count capcodes. 
        /// </summary>
        /// <param name="capcodes"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<IList<JobMessage>> GetMostRecent(IEnumerable<string> capcodes, MessageType messageTypes, int count, int skip)
        {
            return await _repository.GetMostRecent(capcodes, messageTypes, count, skip);
        }

        /// <summary>
        /// Gets the most recent count capcodes. 
        /// </summary>
        /// <param name="capcodes"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<IList<JobMessage>> GetMostRecent(MessageType messageTypes, int count, int skip)
        {
            return await _repository.GetMostRecent(null, messageTypes, count, skip);
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
		/// ///  Gets the job messages for the list of capcodes specified between the specific dates. Results are limited to count number of items.
		/// </summary>
		/// <param name="capcodes"></param>
		/// <param name="messageTypes"></param>
		/// <param name="count"></param>
		/// <param name="skip"></param>
		/// <param name="dateFrom"></param>
		/// <param name="dateTo"></param>
		/// <returns></returns>
		public async Task<IList<JobMessage>> GetMessagesBetweenDates(IEnumerable<string> capcodes, MessageType messageTypes, int count, int skip, DateTime? dateFrom, DateTime? dateTo)
		{
			return await _repository.GetMessagesBetweenDates(capcodes, messageTypes, count, skip, dateFrom, dateTo);
        }

        /// <summary>
        /// ///  Gets the job messages for the list of capcodes specified between the specific dates. Results are limited to count number of items.
        /// </summary>
        /// <param name="capcodes"></param>
        /// <param name="messageTypes"></param>
        /// <param name="count"></param>
        /// <param name="skip"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public async Task<IList<JobMessage>> GetMessagesBetweenDates(MessageType messageTypes, int count, int skip, DateTime? dateFrom, DateTime? dateTo)
        {
            return await _repository.GetMessagesBetweenDates(null, messageTypes, count, skip, dateFrom, dateTo);
        }

        /// <summary>
        /// Gets the job messages for the list of capcodes specified between the specific dates. Results are limited to count number of items.
        /// </summary>
        /// <param name="capcodes"></param>
        /// <param name="messageTypes"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public async Task<IList<Guid>> GetMessageIdsBetweenDates(IEnumerable<string> capcodes, MessageType messageTypes, DateTime? dateFrom, DateTime? dateTo)
		{
			return await _repository.GetMessageIdsBetweenDates(capcodes, messageTypes, dateFrom, dateTo);
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

		public async Task<IList<KeyValuePair<string, Guid>>> GetJobMessageIdsFromCapcodeJobNumbers(IList<KeyValuePair<string, string>> capcodeJobNumbers)
		{
			return await _repository.GetJobMessageIdsFromCapcodeJobNumbers(capcodeJobNumbers);
		}

		public async Task AddAdditionalMessages(IList<KeyValuePair<Guid, AdditionalMessage>> additionalMessages)
		{
			await _repository.AddAdditionalMessages(additionalMessages);
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
			if (message != null)
			{
				message.Notes = message.Notes.OrderByDescending(i => i.Created).ToList();
			}

			// return the message
			return message;
		}

		/// <summary>
		/// Gets the job messages by the Ids specified.
		/// </summary>
		/// <param name="ids">The Ids of the jobs to return.</param>
		/// <returns>The job messages found.</returns>
		public async Task<IList<JobMessage>> GetByIds(IEnumerable<Guid> ids)
		{
			// Return the list of job messages from the database.
			IList<JobMessage> messages = await _repository.GetByIds(ids);
			
			// return the message
			return messages;
		}



        /// <summary>
        /// Gets a list of JobMessages the user has interacted with in some way.
        /// </summary>
        /// <param name="userId">The id of the user to get the jobs for.</param>
        /// <returns>The list of job messages.</returns>
        public async Task<IList<JobMessage>> GetByUserId(Guid userId, int count, int skip)
        {

            // Get the job ids for the user signins
            IList<SignInEntry> userSignIns = await _signInRepository.GetSignInsForUser(userId, SignInType.Operation);
            IList<Guid> jobIds = userSignIns.Select(i => i.OperationDetails.JobId).ToList();

            // Get the JobIds for the attachments
            IList<Attachment> attachments = await _attachmentRepository.GetAttachmentsByUserId(userId);
            IList<Guid> attachmentIds = attachments.Select(i => i.Id).ToList();

            // Return the list of job messages from the database.
            IList<JobMessage> messages = await _repository.GetByUserId(userId, jobIds, attachmentIds, count, skip);

            // return the message
            return messages;
        }

        /// <summary>
        /// Gets the specific job message by the job number.
        /// </summary>
        /// <param name="id">The number of the job to return.</param>
        /// <returns>The job message if found, otherwise null.</returns>
        public async Task<JobMessage> GetByJobNumber(string jobNumber)
		{
			JobMessage message = await _repository.GetByJobNumber(jobNumber);

			// Ensure the notes always come newest first.
			if (message != null)
			{
				message.Notes = message.Notes.OrderByDescending(i => i.Created).ToList();
			}

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
		public async Task<MessageProgress> SaveProgress(Guid jobMessageId, DateTime progressDateTime, Guid userId, MessageProgressType progressType)
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
				Timestamp = progressDateTime.ToUniversalTime(),
				UserId = userId
			};

			// Update the progress in the repository
			await _repository.SaveProgress(jobMessageId, progress);

			// return the progress.
			return progress;

		}

		/// <summary>
		/// Removes the specified progress update type from the job.
		/// </summary>
		/// <param name="jobMessageId">The id of the job to remove the progres from.</param>
		/// <param name="progressType">The progress type to remove.</param>
		public async Task RemoveProgress(Guid jobMessageId, MessageProgressType progressType)
		{
			await _repository.RemoveProgress(jobMessageId, progressType);
		}

		public async Task<PagedResultSet<JobMessage>> FindByKeyword(string keyword, IEnumerable<string> capcodes, MessageType messageTypes, DateTime dateFrom, DateTime dateTo, int limit, int skip, bool countTotal)
		{
			return await _repository.FindByKeyword(keyword, capcodes, messageTypes, dateFrom, dateTo, limit, skip, countTotal);
		}

		/// <summary>
		/// Adds the specified attachment id to the job attachment list.
		/// </summary>
		/// <param name="jobMessageId">The ID of the job to store the attachment against.</param>
		/// <param name="attachmentId">The ID of the attachment to store.</param>
		/// <returns></returns>
		public async Task AddAttachmentToJob(Guid jobMessageId, Guid attachmentId)
		{
			await _repository.AddAttachmentToJob(jobMessageId, attachmentId);
		}

		public async Task RemoveAttachmentFromJob(Guid jobMessageId, Guid attachmentId)
		{
			await _repository.RemoveAttachmentFromJob(jobMessageId, attachmentId);
		}
		
	}
}
