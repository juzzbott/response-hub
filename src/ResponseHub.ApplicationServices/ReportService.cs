using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Enivate.ResponseHub.Model.PdfGeneration.Interface;
using Enivate.ResponseHub.Model.Reports.Interface;
using Enivate.ResponseHub.Common.Constants;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class ReportService : IReportService
	{


		private IPdfGenerationService _pdfGenerationService;

		public ReportService(IPdfGenerationService pdfGenerationService)
		{
			_pdfGenerationService = pdfGenerationService;
		}

		public async Task<byte[]> GenerateTrainingReportPdfFile(Guid groupId, DateTime dateFrom, DateTime dateTo, HttpCookieCollection cookies)
		{
			// Get the web response for the report
			// To force a page break: style="page-break-before: always"
			HttpWebRequest request = HttpWebRequest.CreateHttp(String.Format("{0}/control-panel/reports/generate-training-report-html?group_id={1}&date_from={2}&date_to={3}",
				ConfigurationManager.AppSettings[ConfigurationKeys.BaseWebsiteUrl],
				groupId,
				dateFrom.ToString("yyyyMMddHHmmss"),
				dateTo.ToString("yyyyMMddHHmmss")));

			HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

			// If the response is not successful, then throw exception
			if (response.StatusCode != HttpStatusCode.OK)
			{
				throw new Exception("There was an error response from the Generate PDF Export request.");
			}

			// Get the test from the response
			string htmlContent = "";
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			{
				htmlContent = reader.ReadToEnd();
			}

			// Return the pdf bytes
			return await _pdfGenerationService.GeneratePdfFromHtml(htmlContent, false, cookies);

		}
		
		/// <summary>
		/// Builds the operations report pdf file.
		/// </summary>
		/// <param name="groupId"></param>
		/// <param name="dateFrom"></param>
		/// <param name="dateTo"></param>
		/// <returns></returns>
		public async Task<byte[]> GenerationOperationsReportPdfFile(Guid groupId, DateTime dateFrom, DateTime dateTo, HttpCookieCollection cookies)
		{

			// Get the web response for the report
			// To force a page break: style="page-break-before: always"
			HttpWebRequest request = HttpWebRequest.CreateHttp(String.Format("{0}/control-panel/reports/generate-operations-report-html?group_id={1}&date_from={2}&date_to={3}",
				ConfigurationManager.AppSettings[ConfigurationKeys.BaseWebsiteUrl],
				groupId,
				dateFrom.ToString("yyyyMMddHHmmss"),
				dateTo.ToString("yyyyMMddHHmmss")));

			HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

			// If the response is not successful, then throw exception
			if (response.StatusCode != HttpStatusCode.OK)
			{
				throw new Exception("There was an error response from the Generate PDF Export request.");
			}

			// Store the html string in a variable
			string htmlContent = "";

			// Get the test from the response
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			{
				htmlContent = reader.ReadToEnd();
			}

			// Return the pdf bytes
			return await _pdfGenerationService.GeneratePdfFromHtml(htmlContent, true, cookies);

		}

	}
}
