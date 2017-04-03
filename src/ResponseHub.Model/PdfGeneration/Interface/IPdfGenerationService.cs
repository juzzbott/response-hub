using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.PdfGeneration.Interface
{
	public interface IPdfGenerationService
	{

		Task<byte[]> GeneratePdfFromHtml(string htmlContent, bool portraitLayout);

	}
}
