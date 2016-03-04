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

	}
}
