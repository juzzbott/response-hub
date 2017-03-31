using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Filters;
using System.Threading.Tasks;
using System.Globalization;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.SignIn.Interface;
using Enivate.ResponseHub.Model.SignIn;
using Enivate.ResponseHub.Model.Training;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Training;
using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.Reports.Interface;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{

	[RouteArea("ControlPanel", AreaPrefix = "control-panel")]
	[RoutePrefix("reports")]
	[ClaimsAuthorize(Roles = RoleTypes.GroupAdministrator)]
	public class ReportsController : BaseControlPanelController
	{

		protected ISignInEntryService SignInService
		{
			get
			{
				return ServiceLocator.Get<ISignInEntryService>();
			}
		}

		protected IReportService ReportService
		{
			get
			{
				return ServiceLocator.Get<IReportService>();
			}
		}

		[Route]
		// GET: ControlPanel/DataExport
		public ActionResult Index()
		{
			return View();
		}

		[Route("training-report")]
		public ActionResult TrainingReport()
		{
			return View();
		}

		[Route("training-report")]
		[HttpPost]
		public async Task<ActionResult> TrainingReport(ReportFilterViewModel model)
		{
			
			// Get the list of jobs between the start and end dates
			DateTime dateFrom = model.DateFrom.Date;
			DateTime dateTo = new DateTime(model.DateTo.Year, model.DateTo.Month, model.DateTo.Day, 23, 59, 59);

			if (model.ReportFormat.ToLower() == "display")
			{
				TrainingReportViewModel reportViewModel = await GetTrainingReportModel(GetControlPanelGroupId(), dateFrom, dateTo);
				reportViewModel.UseStandardLayout = true;
				return View("GenerateTrainingReportHtml", reportViewModel);
			}
			else if (model.ReportFormat.ToLower() == "pdf")
			{
				// Get the PDF bytes
				byte[] pdfBytes = await ReportService.GenerateTrainingReportPdfFile(GetControlPanelGroupId(), dateFrom, dateTo);

				FileContentResult result = new FileContentResult(pdfBytes, "application/pdf");
				result.FileDownloadName = String.Format("training-report-{0}.pdf", DateTime.Now.ToString("yyyy-MM-dd"));
				return result;
			}
			else
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "Bad request.");
			}
		}

		[Route("operations-report")]
		public ActionResult OperationsReport()
		{
			return View();
		}

		[Route("generate-training-report-html")]
		[HttpGet]
		[AllowAnonymous]
		public async Task<ActionResult> GenerateTrainingReportHtml()
		{

			// Get the parameters from the query string
			Guid groupId = new Guid(Request.QueryString["group_id"]);
			DateTime dateFrom = DateTime.ParseExact(Request.QueryString["date_from"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
			DateTime dateTo = DateTime.ParseExact(Request.QueryString["date_to"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);

			TrainingReportViewModel model = await GetTrainingReportModel(groupId, dateFrom, dateTo);

			return View(model);

		}

		/// <summary>
		/// Gets the training report model to display to the page.
		/// </summary>
		/// <param name="groupId"></param>
		/// <param name="dateFrom"></param>
		/// <param name="dateTo"></param>
		/// <returns></returns>
		private async Task<TrainingReportViewModel> GetTrainingReportModel(Guid groupId, DateTime dateFrom, DateTime dateTo)
		{
			

			StringBuilder sbChartOptionsJs = new StringBuilder();

			// Create the model
			TrainingReportViewModel model = new TrainingReportViewModel()
			{
				ChartDataJs = "",
				ChartOptionsJs = sbChartOptionsJs.ToString(),
				MemberReports = new List<GroupTrainingMemberReportItem>(),
				DateFrom = dateFrom,
				DateTo = dateTo
			};

			// Get the training types
			foreach (TrainingType trainingType in Enum.GetValues(typeof(TrainingType)))
			{
				model.TrainingTypes.Add(trainingType, trainingType.GetEnumDescription());
			}

			return model;
		}
	}
}