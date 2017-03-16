using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Decoding;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface IDecoderStatusRepository
	{

		Task<DecoderStatus> GetDecoderStatus();

		Task SetLastCleanupOperationTimestamp(DateTime timestamp);

		Task SetLastEmailWarningTimestamp(DateTime timestamp);

		Task AddInvalidMessage(DateTime timestamp, string invalidMessage);

		Task ClearInvalidMessages();

		Task ResetDecoderStatus();

	}
}
