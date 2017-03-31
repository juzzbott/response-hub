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
using Enivate.ResponseHub.Model.SignIn;
using Enivate.ResponseHub.Model.SignIn.Interface;
using Enivate.ResponseHub.Model.Training;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Training;
using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.Reports.Interface;
using Enivate.ResponseHub.Model.Training.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Operations;
using Enivate.ResponseHub.Model.Groups;

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

		public ITrainingSessionService TrainingSessionService
		{
			get
			{
				return ServiceLocator.Get<ITrainingSessionService>();
			}
		}

		protected IJobMessageService JobMessageService
		{
			get
			{
				return ServiceLocator.Get<IJobMessageService>();
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

		[Route("operations-report")]
		[HttpPost]
		public async Task<ActionResult> OperationsReport(ReportFilterViewModel model)
		{
			// Get the list of jobs between the start and end dates
			DateTime dateFrom = model.DateFrom.Date;
			DateTime dateTo = new DateTime(model.DateTo.Year, model.DateTo.Month, model.DateTo.Day, 23, 59, 59);

			if (model.ReportFormat.ToLower() == "display")
			{
				OperationsReportViewModel reportViewModel = await GetOperationsReportModel(GetControlPanelGroupId(), dateFrom, dateTo);
				reportViewModel.UseStandardLayout = true;
				return View("GenerateOperationsReportHtml", reportViewModel);
			}
			else if (model.ReportFormat.ToLower() == "pdf")
			{
				// Get the PDF bytes
				byte[] pdfBytes = await ReportService.GenerationOperationsReportPdfFile(GetControlPanelGroupId(), dateFrom, dateTo);

				FileContentResult result = new FileContentResult(pdfBytes, "application/pdf");
				result.FileDownloadName = String.Format("operations-report-{0}.pdf", DateTime.Now.ToString("yyyy-MM-dd"));
				return result;
			}
			else
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "Bad request.");
			}
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

			// Get the training report model
			TrainingReportViewModel model = await GetTrainingReportModel(groupId, dateFrom, dateTo);

			return View(model);

		}

		[Route("generate-operations-report-html")]
		[HttpGet]
		[AllowAnonymous]
		public async Task<ActionResult> GenerateOperationsReportHtml()
		{

			// Get the parameters from the query string
			Guid groupId = new Guid(Request.QueryString["group_id"]);
			DateTime dateFrom = DateTime.ParseExact(Request.QueryString["date_from"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
			DateTime dateTo = DateTime.ParseExact(Request.QueryString["date_to"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);

			// Get the group by the id
			Group group = await GroupService.GetById(groupId);

			// Ensure the user is a group administrator of the specific group, otherwise 403 forbidden.
			
			// Get the operations report model
			OperationsReportViewModel model = await GetOperationsReportModel(group.Id, dateFrom, dateTo);

			// return the model
			return View(model);

		}

		/// <summary>
		/// Gets the operations report model to display the page.
		/// </summary>
		/// <param name="dateFrom"></param>
		/// <param name="dateTo"></param>
		/// <param name="group"></param>
		/// <returns></returns>
		private async Task<OperationsReportViewModel> GetOperationsReportModel(Guid groupId, DateTime dateFrom, DateTime dateTo)
		{

			// Get the group by the id
			Group group = await GroupService.GetById(groupId);

			// Get the list of messages for the capcode
			IList<JobMessage> jobMessages = await JobMessageService.GetJobMessagesBetweenDates(
				new List<string> { group.Capcode },
				MessageType.Job & MessageType.Message,
				dateFrom,
				dateTo);

			// Create the model
			OperationsReportViewModel model = new OperationsReportViewModel
			{
				Messages = jobMessages.Where(i => i.Type == MessageType.Message).ToList(),
				Jobs = jobMessages.Where(i => i.Type == MessageType.Job).OrderBy(i => i.Priority).ToList(),
				StartDate = dateFrom,
				FinishDate = dateTo
			};

			// return the model
			return model;
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
			// Get the training sessions
			IList<TrainingSession> trainingSessions = await TrainingSessionService.GetTrainingSessionsForGroup(groupId);

			// Get the members for the group
			IList<IdentityUser> groupMembers = await GroupService.GetUsersForGroup(groupId);

			// Get the aggregate chart data
			IDictionary<string, int> aggregate = new Dictionary<string, int>();
			foreach (TrainingType trainingType in Enum.GetValues(typeof(TrainingType)))
			{
				aggregate.Add(trainingType.GetEnumDescription(), trainingSessions.Count(i => i.TrainingType == trainingType));
			}

			// Build the chart data
			StringBuilder sbChartData = new StringBuilder();
			sbChartData.Append("{\"labels\": [");
			for (int i = 0; i < aggregate.Count; i++)
			{
				KeyValuePair<string, int> item = aggregate.ElementAt(i);
				sbChartData.AppendFormat("{0}\"{1}\"", (i == 0 ? "" : ","), item.Key);
			}
			sbChartData.Append("],\"series\": [");
			for (int i = 0; i < aggregate.Count; i++)
			{
				KeyValuePair<string, int> item = aggregate.ElementAt(i);
				sbChartData.AppendFormat("{0}{1}", (i == 0 ? "" : ","), item.Value);
			}
			sbChartData.Append("]}");

			StringBuilder sbChartOptionsJs = new StringBuilder();

			// Build the member reports
			IList<GroupTrainingMemberReportItem> groupMemberReports = new List<GroupTrainingMemberReportItem>();

			// Create the member report items
			foreach (IdentityUser user in groupMembers)
			{
				// Create the training item
				GroupTrainingMemberReportItem memberTrainingRecord = new GroupTrainingMemberReportItem()
				{
					Name = user.FullName
				};

				// Get the training session where the user is recorded as either a member or trainer
				IList<TrainingSession> userSessions = trainingSessions.Where(i => i.Members.Contains(user.Id) || i.Trainers.Contains(user.Id)).ToList();

				// Get the training types
				foreach (TrainingType trainingType in Enum.GetValues(typeof(TrainingType)))
				{

					// Get the total amount of training sessions for each session
					memberTrainingRecord.TrainingSessions.Add(trainingType, userSessions.Where(i => i.TrainingType == trainingType).Count());

					// Get the dates the user was training for.
					memberTrainingRecord.TrainingDates.Add(trainingType, userSessions.Where(i => i.TrainingType == trainingType).Select(i => i.SessionDate).ToList());
				}

				// Add to the list of members
				groupMemberReports.Add(memberTrainingRecord);
			}

			// Create the model
			TrainingReportViewModel model = new TrainingReportViewModel()
			{
				ChartDataJs = sbChartData.ToString(),
				ChartOptionsJs = sbChartOptionsJs.ToString(),
				MemberReports = groupMemberReports,
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