using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.PdfGeneration.Interface
{
	public interface IPdfGenerationService
	{

		byte[] GeneratePdfFromHtml(string htmlContent, bool portraitLayout);

	}
}
