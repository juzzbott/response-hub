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

		public async Task<byte[]> GenerateTrainingReportPdfFile(Guid unitId, DateTime dateFrom, DateTime dateTo, Guid? memberId, HttpCookieCollection cookies)
		{

			// Generate the report url
			string url = String.Format("{0}/control-panel/reports/generate-training-report-html?unit_id={1}&date_from={2}&date_to={3}&canvas_to_image=1",
				ConfigurationManager.AppSettings[ConfigurationKeys.BaseWebsiteUrl],
				unitId,
				dateFrom.ToString("yyyyMMddHHmmss"),
				dateTo.ToString("yyyyMMddHHmmss"));

			// If there is a member id, then add it to the query string
			if (memberId.HasValue && memberId.Value != Guid.Empty)
			{
				url = String.Format("{0}&member_id={1}", url, memberId.Value);
			}

			// Get the web response for the report
			// To force a page break: style="page-break-before: always"
			HttpWebRequest request = HttpWebRequest.CreateHttp(url);

			request.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
			{
				return true;
			};

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

        public async Task<byte[]> GenerateTrainingActivityReportPdfFile(Guid unitId, DateTime dateFrom, DateTime dateTo, HttpCookieCollection cookies)
        {

            // Generate the report url
            string url = String.Format("{0}/control-panel/reports/generate-training-activity-report-html?unit_id={1}&date_from={2}&date_to={3}&canvas_to_image=1",
                ConfigurationManager.AppSettings[ConfigurationKeys.BaseWebsiteUrl],
                unitId,
                dateFrom.ToString("yyyyMMddHHmmss"),
                dateTo.ToString("yyyyMMddHHmmss"));

            // Get the web response for the report
            // To force a page break: style="page-break-before: always"
            HttpWebRequest request = HttpWebRequest.CreateHttp(url);

            request.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };

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
            return await _pdfGenerationService.GeneratePdfFromHtml(htmlContent, true, cookies);

        }

        public async Task<byte[]> GenerateTrainersReportPdfFile(Guid unitId, DateTime dateFrom, DateTime dateTo, HttpCookieCollection cookies)
		{
			// Get the web response for the report
			// To force a page break: style="page-break-before: always"
			HttpWebRequest request = HttpWebRequest.CreateHttp(String.Format("{0}/control-panel/reports/generate-trainers-report-html?unit_id={1}&date_from={2}&date_to={3}",
				ConfigurationManager.AppSettings[ConfigurationKeys.BaseWebsiteUrl],
				unitId,
				dateFrom.ToString("yyyyMMddHHmmss"),
				dateTo.ToString("yyyyMMddHHmmss")));

			request.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
			{
				return true;
			};

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

		public async Task<byte[]> GenerateAttendanceReportPdfFile(Guid unitId, DateTime dateFrom, DateTime dateTo, HttpCookieCollection cookies)
		{
			// Get the web response for the report
			// To force a page break: style="page-break-before: always"
			HttpWebRequest request = HttpWebRequest.CreateHttp(String.Format("{0}/control-panel/reports/generate-attendance-report-html?unit_id={1}&date_from={2}&date_to={3}",
				ConfigurationManager.AppSettings[ConfigurationKeys.BaseWebsiteUrl],
				unitId,
				dateFrom.ToString("yyyyMMddHHmmss"),
				dateTo.ToString("yyyyMMddHHmmss")));

			request.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
			{
				return true;
			};

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
		/// <param name="unitId"></param>
		/// <param name="dateFrom"></param>
		/// <param name="dateTo"></param>
		/// <returns></returns>
		public async Task<byte[]> GenerationOperationsReportPdfFile(Guid unitId, DateTime dateFrom, DateTime dateTo, bool includeAdditionalCapcodes, HttpCookieCollection cookies)
		{

			// Get the web response for the report
			// To force a page break: style="page-break-before: always"
			HttpWebRequest request = HttpWebRequest.CreateHttp(String.Format("{0}/control-panel/reports/generate-operations-report-html?unit_id={1}&date_from={2}&date_to={3}&additional_capcodes={4}",
				ConfigurationManager.AppSettings[ConfigurationKeys.BaseWebsiteUrl],
				unitId,
				dateFrom.ToString("yyyyMMddHHmmss"),
				dateTo.ToString("yyyyMMddHHmmss"),
				includeAdditionalCapcodes));

			request.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
			{
				return true;
			};

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
