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

		byte[] BuildPdfExportFile(IList<JobMessage> messages);

	}
}
