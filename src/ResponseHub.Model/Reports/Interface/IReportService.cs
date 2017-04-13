using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Enivate.ResponseHub.Model.Reports.Interface
{
	public interface IReportService
	{

		Task<byte[]> GenerateTrainingReportPdfFile(Guid groupId, DateTime dateFrom, DateTime dateTo, HttpCookieCollection cookies);

		Task<byte[]> GenerationOperationsReportPdfFile(Guid groupId, DateTime dateFrom, DateTime dateTo, HttpCookieCollection cookies);

	}
}
