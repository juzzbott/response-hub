using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EvoPdf;

using Enivate.ResponseHub.Model.PdfGeneration.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class PdfGenerationService : IPdfGenerationService
	{

		/// <summary>
		/// EvoPDF License key
		/// </summary>
		private const string EvoPdfLicenseKey = "SsTXxdDVxdbcxdDL1cXW1MvU18vc3Nzc";

		public byte[] GeneratePdfFromHtml(string htmlContent, bool portraitLayout)
		{
			// Create the converter
			HtmlToPdfConverter converter = new HtmlToPdfConverter();

			//set the PDF document margins
			converter.LicenseKey = EvoPdfLicenseKey;
			converter.PdfDocumentOptions.LeftMargin = 30;
			converter.PdfDocumentOptions.RightMargin = 30;
			converter.PdfDocumentOptions.TopMargin = 30;
			converter.PdfDocumentOptions.BottomMargin = 30;

			// Set the page orientation
			converter.PdfDocumentOptions.PdfPageOrientation = (portraitLayout ? PdfPageOrientation.Portrait : PdfPageOrientation.Landscape);

			// embed the true type fonts in the generated PDF document
			converter.PdfDocumentOptions.EmbedFonts = true;

			// compress the images in PDF with JPEG to reduce the PDF document size
			converter.PdfDocumentOptions.JpegCompressionEnabled = false;

			// return the pdf bytes
			return converter.ConvertHtml(htmlContent, ConfigurationManager.AppSettings["BaseWebsiteUrl"]);
		}
	}
}
