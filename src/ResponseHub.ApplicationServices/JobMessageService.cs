using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.DataAccess.Interface;

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
		/// Gets the most recent count capcodes. 
		/// </summary>
		/// <param name="capcodes"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public async Task<IList<JobMessage>> GetMostRecent(IEnumerable<string> capcodes, int count)
		{
			return await _repository.GetMostRecent(capcodes, count);
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

			// CReate the job note
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

	}
}
