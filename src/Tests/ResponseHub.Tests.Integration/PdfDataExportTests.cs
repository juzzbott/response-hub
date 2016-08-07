using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.DataExport.Interface;
using Enivate.ResponseHub.Tests.Integration.Fixtures;

using Xunit;

namespace Enivate.ResponseHub.Tests.Integration
{
	public class PdfExportTests : IClassFixture<UnityCollectionFixture>
	{

		protected IDataExportService DataExportService
		{
			get
			{
				return ServiceLocator.Get<IDataExportService>();
			}
		}

		[Fact(DisplayName = "Can generate PDF data export")]
		[Trait("Category", "Integration Test - Pdf Data Export")]
		public async Task CanGeneratePdfTest()
		{

			Guid groupId = new Guid("2e54f8ea-25e6-40c5-a64b-7950d84a059c");
			DateTime dateFrom = new DateTime(2016, 01, 01);
			DateTime dateTo = new DateTime(2016, 08, 01, 23, 59, 59);

			// Get the PDF bytes
			byte[] pdfBytes = await DataExportService.BuildPdfExportFile(groupId, dateFrom, dateTo);

			// Get the PDF dir and ensure it exists
			string pdfDir = String.Format("{0}\\PdfDataExport", Environment.CurrentDirectory);
			if (!Directory.Exists(pdfDir))
			{
				Directory.CreateDirectory(pdfDir);
			}

			// Write the PDF file to disk
			File.WriteAllBytes(String.Format("{0}\\pdf-data-export.pdf", pdfDir), pdfBytes);


		}

	}
}
