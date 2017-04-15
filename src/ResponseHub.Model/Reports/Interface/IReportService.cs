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

		Task<byte[]> GenerateTrainingReportPdfFile(Guid unitId, DateTime dateFrom, DateTime dateTo, HttpCookieCollection cookies);

		Task<byte[]> GenerationOperationsReportPdfFile(Guid unitId, DateTime dateFrom, DateTime dateTo, bool includeAdditionalCapcodes, HttpCookieCollection cookies);

	}
}
