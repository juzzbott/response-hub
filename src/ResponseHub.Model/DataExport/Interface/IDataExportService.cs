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

		Task<string> BuildCsvExportFile(Guid groupId, DateTime dateFrom, DateTime dateTo);

		Task<byte[]> BuildPdfExportFile(Guid groupId, DateTime dateFrom, DateTime dateTo);

		Task<string> BuildHtmlExportFile(Guid groupId, DateTime dateFrom, DateTime dateTo);

	}
}
