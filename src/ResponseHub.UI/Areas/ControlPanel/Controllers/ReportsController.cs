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

		protected ISignInEntryService SignInService = ServiceLocator.Get<ISignInEntryService>();
		protected IReportService ReportService = ServiceLocator.Get<IReportService>();
		public ITrainingService TrainingService = ServiceLocator.Get<ITrainingService>();
		protected IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();

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
				byte[] pdfBytes = await ReportService.GenerateTrainingReportPdfFile(GetControlPanelGroupId(), dateFrom, dateTo, Request.Cookies);

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
		public async Task<ActionResult> OperationsReport(OperationsFilterViewModel model)
		{
			// Get the list of jobs between the start and end dates
			DateTime dateFrom = model.DateFrom.Date;
			DateTime dateTo = new DateTime(model.DateTo.Year, model.DateTo.Month, model.DateTo.Day, 23, 59, 59);

			if (model.ReportFormat.ToLower() == "display")
			{
				OperationsReportViewModel reportViewModel = await GetOperationsReportModel(GetControlPanelGroupId(), dateFrom, dateTo, model.IncludeAdditionalCapcodes);
				reportViewModel.UseStandardLayout = true;
				return View("GenerateOperationsReportHtml", reportViewModel);
			}
			else if (model.ReportFormat.ToLower() == "pdf")
			{
				// Get the PDF bytes
				byte[] pdfBytes = await ReportService.GenerationOperationsReportPdfFile(GetControlPanelGroupId(), dateFrom, dateTo, model.IncludeAdditionalCapcodes, Request.Cookies);

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
			bool includeAdditionalCapcodes = Boolean.Parse(Request.QueryString["additional_capcodes"]);

			// Get the group by the id
			Group group = await GroupService.GetById(groupId);

			// Ensure the user is a group administrator of the specific group, otherwise 403 forbidden.
			
			// Get the operations report model
			OperationsReportViewModel model = await GetOperationsReportModel(group.Id, dateFrom, dateTo, includeAdditionalCapcodes);

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
		private async Task<OperationsReportViewModel> GetOperationsReportModel(Guid groupId, DateTime dateFrom, DateTime dateTo, bool includeAllCapcodes)
		{

			// Get the group by the id
			Group group = await GroupService.GetById(groupId);

			// Create the list of capcodes
			List<Capcode> selectedCapcodes = new List<Capcode> { new Capcode() { CapcodeAddress = group.Capcode } };

			// Get all the capcodes for the group if we are to include additional capcodes
			if (includeAllCapcodes)
			{
				// Get all the capcodes and select the group additional capcodes for the group
				IList<Capcode> allCapcodes = await CapcodeService.GetAll();
				selectedCapcodes.AddRange(allCapcodes.Where(i => group.AdditionalCapcodes.Contains(i.Id)));
			}

			// Get the list of messages for the capcode
			IList<JobMessage> jobMessages = await JobMessageService.GetMessagesBetweenDates(
				selectedCapcodes,
				MessageType.Job & MessageType.Message,
				999999, 
				0,
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
			IList<TrainingSession> trainingSessions = await TrainingService.GetTrainingSessionsForGroup(groupId);
			int trainingSessionDays = trainingSessions.GroupBy(i => i.SessionDate).Count();

			// Get the members for the group
			IList<IdentityUser> groupMembers = await GroupService.GetUsersForGroup(groupId);

			// Get the training types.
			IList<TrainingType> trainingTypes = await TrainingService.GetAllTrainingTypes();

			// Get the aggregate chart data
			IDictionary<string, int> aggregate = new Dictionary<string, int>();
			foreach (TrainingType trainingType in trainingTypes)
			{
				aggregate.Add(trainingType.ShortName, trainingSessions.Count(i => i.TrainingTypes.Contains(trainingType)));
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

				// Get the percentage of attendance
				if (trainingSessions.Count > 0)
				{
					memberTrainingRecord.AttendancePercent = (int)(((decimal)userSessions.Count / (decimal)trainingSessionDays) * 100);
				}

				// Get the training types
				foreach (TrainingType trainingType in trainingTypes)
				{

					// Get the total amount of training sessions for each session
					memberTrainingRecord.TrainingSessions.Add(trainingType.ShortName, userSessions.Where(i => i.TrainingTypes.Contains(trainingType)).Count());

					// Get the dates the user was training for.
					memberTrainingRecord.TrainingDates.Add(trainingType.ShortName, userSessions.Where(i => i.TrainingTypes.Contains(trainingType)).Select(i => i.SessionDate).ToList());
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
			foreach (TrainingType trainingType in trainingTypes)
			{
				model.TrainingTypes.Add(trainingType, trainingType.ShortName);
			}

			return model;
		}
	}
}