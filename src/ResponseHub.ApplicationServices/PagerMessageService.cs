using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class PagerMessageService : IPagerMessageService
	{

		private IPagerMessageRepository _pagerMessageRepository;

		private ILogger _log;

		/// <summary>
		/// Creates a new instance of the PagerMessageService.
		/// </summary>
		/// <param name="pagerMessageRepository">The pager message repsitory.</param>
		/// <param name="log">The logger.</param>
		public PagerMessageService(IPagerMessageRepository pagerMessageRepository, ILogger log)
		{
			_pagerMessageRepository = pagerMessageRepository;
			_log = log;
		}

		/// <summary>
		/// Saves the raw pager message into the database.
		/// </summary>
		/// <param name="pagerMessage">The pager message to save into the database.</param>
		/// <returns></returns>
		public async Task Save(PagerMessage pagerMessage)
		{
			// Save the pager message into the database.
			await _pagerMessageRepository.Save(pagerMessage);
		}
	}
}
