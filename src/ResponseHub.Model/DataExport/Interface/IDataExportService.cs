using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.Model.DataExport.Interface
{
	public interface IDataExportService
	{

		string BuildCsvExportFile(IList<JobMessage> messages);

		Task<byte[]> BuildPdfExportFile(Guid groupId, DateTime dateFrom, DateTime dateTo);

	}
}
