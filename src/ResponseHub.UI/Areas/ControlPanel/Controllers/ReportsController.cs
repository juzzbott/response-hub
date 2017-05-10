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
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Attendance;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{

	[RouteArea("ControlPanel", AreaPrefix = "control-panel")]
	[RoutePrefix("reports")]
	[ClaimsAuthorize(Roles = RoleTypes.UnitAdministrator)]
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

		#region Training report

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
				TrainingReportViewModel reportViewModel = await GetTrainingReportModel(GetControlPanelUnitId(), dateFrom, dateTo);
				reportViewModel.UseStandardLayout = true;
				return View("GenerateTrainingReportHtml", reportViewModel);
			}
			else if (model.ReportFormat.ToLower() == "pdf")
			{
				// Get the PDF bytes
				byte[] pdfBytes = await ReportService.GenerateTrainingReportPdfFile(GetControlPanelUnitId(), dateFrom, dateTo, Request.Cookies);

				FileContentResult result = new FileContentResult(pdfBytes, "application/pdf");
				result.FileDownloadName = String.Format("training-report-{0}.pdf", DateTime.Now.ToString("yyyy-MM-dd"));
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
			Guid unitId = new Guid(Request.QueryString["unit_id"]);
			DateTime dateFrom = DateTime.ParseExact(Request.QueryString["date_from"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
			DateTime dateTo = DateTime.ParseExact(Request.QueryString["date_to"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);

			// Get the training report model
			TrainingReportViewModel model = await GetTrainingReportModel(unitId, dateFrom, dateTo);

			return View(model);

		}

		#endregion

		#region Operations report

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
				OperationsReportViewModel reportViewModel = await GetOperationsReportModel(GetControlPanelUnitId(), dateFrom, dateTo, model.IncludeAdditionalCapcodes);
				reportViewModel.UseStandardLayout = true;
				return View("GenerateOperationsReportHtml", reportViewModel);
			}
			else if (model.ReportFormat.ToLower() == "pdf")
			{
				// Get the PDF bytes
				byte[] pdfBytes = await ReportService.GenerationOperationsReportPdfFile(GetControlPanelUnitId(), dateFrom, dateTo, model.IncludeAdditionalCapcodes, Request.Cookies);

				FileContentResult result = new FileContentResult(pdfBytes, "application/pdf");
				result.FileDownloadName = String.Format("operations-report-{0}.pdf", DateTime.Now.ToString("yyyy-MM-dd"));
				return result;
			}
			else
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "Bad request.");
			}
		}

		[Route("generate-operations-report-html")]
		[HttpGet]
		[AllowAnonymous]
		public async Task<ActionResult> GenerateOperationsReportHtml()
		{

			// Get the parameters from the query string
			Guid unitId = new Guid(Request.QueryString["unit_id"]);
			DateTime dateFrom = DateTime.ParseExact(Request.QueryString["date_from"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
			DateTime dateTo = DateTime.ParseExact(Request.QueryString["date_to"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
			bool includeAdditionalCapcodes = Boolean.Parse(Request.QueryString["additional_capcodes"]);

			// Get the unit by the id
			Unit unit = await UnitService.GetById(unitId);

			// Ensure the user is an administrator of the specific unit, otherwise 403 forbidden.
			
			// Get the operations report model
			OperationsReportViewModel model = await GetOperationsReportModel(unit.Id, dateFrom, dateTo, includeAdditionalCapcodes);

			// return the model
			return View(model);

		}

		#endregion

		#region Attendance report
		
		[Route("attendance-report")]
		public ActionResult AttendanceReport()
		{
			return View();
		}

		[Route("attendance-report")]
		[HttpPost]
		public async Task<ActionResult> AttendanceReport(ReportFilterViewModel model)
		{

			// Get the list of jobs between the start and end dates
			DateTime dateFrom = model.DateFrom.Date;
			DateTime dateTo = new DateTime(model.DateTo.Year, model.DateTo.Month, model.DateTo.Day, 23, 59, 59);

			if (model.ReportFormat.ToLower() == "display")
			{
				AttendanceReportViewModel reportViewModel = await GetAttendanceReportModel(GetControlPanelUnitId(), dateFrom, dateTo);
				reportViewModel.UseStandardLayout = true;
				return View("GenerateAttendanceReportHtml", reportViewModel);
			}
			else if (model.ReportFormat.ToLower() == "pdf")
			{
				// Get the PDF bytes
				byte[] pdfBytes = await ReportService.GenerateAttendanceReportPdfFile(GetControlPanelUnitId(), dateFrom, dateTo, Request.Cookies);

				FileContentResult result = new FileContentResult(pdfBytes, "application/pdf");
				result.FileDownloadName = String.Format("attendance-report-{0}.pdf", DateTime.Now.ToString("yyyy-MM-dd"));
				return result;
			}
			else if (model.ReportFormat.ToLower() == "csv")
			{
				// Get the csv string
				string csvData = await GenerateAttendanceReportCsv(GetControlPanelUnitId(), dateFrom, dateTo);

				FileContentResult result = new FileContentResult(Encoding.UTF8.GetBytes(csvData), "text/csv");
				result.FileDownloadName = String.Format("attendance-report-{0}.csv", DateTime.Now.ToString("yyyy-MM-dd"));
				return result;
			}
			else
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "Bad request.");
			}
		}

		[Route("generate-attendance-report-html")]
		[HttpGet]
		[AllowAnonymous]
		public async Task<ActionResult> GenerateAttendanceReportHtml()
		{

			// Get the parameters from the query string
			Guid unitId = new Guid(Request.QueryString["unit_id"]);
			DateTime dateFrom = DateTime.ParseExact(Request.QueryString["date_from"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
			DateTime dateTo = DateTime.ParseExact(Request.QueryString["date_to"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);

			// Get the unit by the id
			Unit unit = await UnitService.GetById(unitId);

			// Ensure the user is an administrator of the specific unit, otherwise 403 forbidden.

			// Get the operations report model
			AttendanceReportViewModel model = await GetAttendanceReportModel(unit.Id, dateFrom, dateTo);

			// return the model
			return View(model);

		}

		/// <summary>
		/// Gets the CSV data for the attendance report.
		/// </summary>
		/// <param name="unitId"></param>
		/// <param name="dateFrom"></param>
		/// <param name="dateTo"></param>
		/// <returns></returns>
		public async Task<string> GenerateAttendanceReportCsv(Guid unitId, DateTime dateFrom, DateTime dateTo)
		{

			// Get the report view model
			AttendanceReportViewModel reportViewModel = await GetAttendanceReportModel(GetControlPanelUnitId(), dateFrom, dateTo);

			// Get all the signins from the month groups
			IList<MemberAttendanceReportItem> reportItems = reportViewModel.AttendanceReportItems.SelectMany(i => i.Value).ToList();

			// Create the string builder to store the csv data in
			StringBuilder sbCsvData = new StringBuilder();
			sbCsvData.AppendLine("Date,Time,Name,Member number,Type,Description");

			// Add the individual records
			foreach(MemberAttendanceReportItem reportItem in reportItems)
			{
				sbCsvData.AppendLine(String.Format("{0},{1},{2},{3},{4},{5}",
					reportItem.SignInTime.ToString("yyyy-MM-dd"),
					reportItem.SignInTime.ToString("HH:mm"),
					reportItem.FullName,
					reportItem.MemberNumber,
					reportItem.SignInType.GetEnumDescription(),
					reportItem.Description));
			}

			// return the csv data
			return sbCsvData.ToString();

		}

		#endregion

		#region Helpers

		/// <summary>
		/// Gets the operations report model to display the page.
		/// </summary>
		/// <param name="dateFrom"></param>
		/// <param name="dateTo"></param>
		/// <param name="unit"></param>
		/// <returns></returns>
		private async Task<OperationsReportViewModel> GetOperationsReportModel(Guid unitId, DateTime dateFrom, DateTime dateTo, bool includeAllCapcodes)
		{

			// Get the unit by the id
			Unit unit = await UnitService.GetById(unitId);

			// Create the list of capcodes
			List<Capcode> selectedCapcodes = new List<Capcode> { new Capcode() { CapcodeAddress = unit.Capcode } };

			// Get all the capcodes for the unit if we are to include additional capcodes
			if (includeAllCapcodes)
			{
				// Get all the capcodes and select the unit additional capcodes for the unit
				IList<Capcode> allCapcodes = await CapcodeService.GetAll();
				selectedCapcodes.AddRange(allCapcodes.Where(i => unit.AdditionalCapcodes.Contains(i.Id)));
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
				Messages = jobMessages.Where(i => i.Type == MessageType.Message).OrderByDescending(i => i.Timestamp).ToList(),
				Jobs = jobMessages.Where(i => i.Type == MessageType.Job).OrderByDescending(i => i.Timestamp).ToList(),
				StartDate = dateFrom,
				FinishDate = dateTo
			};

			// return the model
			return model;
		}


		/// <summary>
		/// Gets the training report model to display to the page.
		/// </summary>
		/// <param name="unitId"></param>
		/// <param name="dateFrom"></param>
		/// <param name="dateTo"></param>
		/// <returns></returns>
		private async Task<TrainingReportViewModel> GetTrainingReportModel(Guid unitId, DateTime dateFrom, DateTime dateTo)
		{
			// Get the training sessions
			IList<TrainingSession> trainingSessions = await TrainingService.GetTrainingSessionsForUnit(unitId, dateFrom, dateTo);
			int trainingSessionDays = trainingSessions.GroupBy(i => i.SessionDate.Date).Count();

			// Get the members for the unit
			IList<IdentityUser> unitMembers = await UnitService.GetUsersForUnit(unitId);

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
			IList<UnitTrainingMemberReportItem> unitMemberReports = new List<UnitTrainingMemberReportItem>();

			// Create the member report items
			foreach (IdentityUser user in unitMembers)
			{
				// Create the training item
				UnitTrainingMemberReportItem memberTrainingRecord = new UnitTrainingMemberReportItem()
				{
					Name = user.FullName
				};

				// Get the training session where the user is recorded as either a member or trainer
				IList<TrainingSession> userSessions = trainingSessions.Where(i => i.Members.Contains(user.Id) || i.Trainers.Contains(user.Id)).Distinct().ToList();

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
				}

				// Add to the list of members
				unitMemberReports.Add(memberTrainingRecord);
			}

			// Create the model
			TrainingReportViewModel model = new TrainingReportViewModel()
			{
				ChartDataJs = sbChartData.ToString(),
				ChartOptionsJs = sbChartOptionsJs.ToString(),
				MemberReports = unitMemberReports,
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

		/// <summary>
		/// Gets the training report model to display to the page.
		/// </summary>
		/// <param name="unitId"></param>
		/// <param name="dateFrom"></param>
		/// <param name="dateTo"></param>
		/// <returns></returns>
		private async Task<AttendanceReportViewModel> GetAttendanceReportModel(Guid unitId, DateTime dateFrom, DateTime dateTo)
		{

			// Get the user sign ins for the unit
			IList<SignInEntry> unitSignIns = await SignInService.GetSignInsForUnit(unitId, dateFrom, dateTo);

			// Get the training sessions
			IList<TrainingSession> trainingSessions = await TrainingService.GetTrainingSessionsForUnit(unitId, dateFrom, dateTo);

			// Get the user ids in the sign ins
			List<Guid> userIds = unitSignIns.Select(i => i.UserId).Distinct().ToList();
			userIds.AddRange(trainingSessions.SelectMany(i => i.Members).Distinct());
			userIds.AddRange(trainingSessions.SelectMany(i => i.Trainers).Distinct());
			userIds = userIds.Distinct().ToList();

			// Get the users for the signins
			IList<IdentityUser> signInUsers = await UserService.GetUsersByIds(userIds);

			// Create the list of report items
			List<MemberAttendanceReportItem> reportItems = new List<MemberAttendanceReportItem>();
			reportItems.AddRange(unitSignIns.Select(i => MapMemberAttendanceReportItem(i, signInUsers)));
			reportItems.AddRange(trainingSessions.SelectMany(i => MapMemberAttendanceReportItem(i, signInUsers)));

			// Sort by sign in time desc
			reportItems = reportItems.OrderByDescending(i => i.SignInTime).ToList();

			// Create the list of the sign in entry report items
			IDictionary<string, List<MemberAttendanceReportItem>> groupedReportItems = new Dictionary<string, List<MemberAttendanceReportItem>>();

			// Loop through the sign ins
			foreach(MemberAttendanceReportItem reportItem in reportItems)
			{
				// Get the month year string for the sign in date
				string monthYear = reportItem.SignInTime.ToLocalTime().ToString("MMMM yyyy");
				
				// If the key doesn't exist in the dictionary, add it
				if (!groupedReportItems.ContainsKey(monthYear))
				{
					groupedReportItems.Add(monthYear, new List<MemberAttendanceReportItem>());
				}
				
				// Add the report item to the list
				groupedReportItems[monthYear].Add(reportItem);

			}
			
			// Create the model
			AttendanceReportViewModel model = new AttendanceReportViewModel()
			{
				//ChartDataJs = sbChartData.ToString(),
				//ChartOptionsJs = sbChartOptionsJs.ToString(),
				AttendanceReportItems = groupedReportItems,
				DateFrom = dateFrom,
				DateTo = dateTo
			};

			return model;
		}

		private IList<MemberAttendanceReportItem> MapMemberAttendanceReportItem(TrainingSession trainingSession, IList<IdentityUser> signInUsers)
		{

			// Create the list of report items
			IList<MemberAttendanceReportItem> reportItems = new List<MemberAttendanceReportItem>();

			// Get the session description
			string description = String.Join(", ", trainingSession.TrainingTypes.Select(i => i.Name));

			// Loop through the members in the training sessions
			foreach (Guid userId in trainingSession.Members)
			{

				// Get the user from the list
				IdentityUser user = signInUsers.FirstOrDefault(i => i.Id == userId);

				// If the user is not null, get the report item and add to the list
				if (user != null)
				{

					MemberAttendanceReportItem reportItem = new MemberAttendanceReportItem()
					{
						SignInTime = trainingSession.SessionDate.ToLocalTime(),
						SignInType = SignInType.Training,
						Description = description,
						FullName = user.FullName,
						MemberNumber = user.Profile.MemberNumber
					};
					reportItems.Add(reportItem);
				}
			}

			// Loop through the members in the training sessions
			foreach (Guid userId in trainingSession.Trainers)
			{

				// Get the user from the list
				IdentityUser user = signInUsers.FirstOrDefault(i => i.Id == userId);

				// If the user is not null, get the report item and add to the list
				if (user != null)
				{

					MemberAttendanceReportItem reportItem = new MemberAttendanceReportItem()
					{
						SignInTime = trainingSession.SessionDate.ToLocalTime(),
						SignInType = SignInType.Training,
						Description = description,
						FullName = user.FullName,
						MemberNumber = user.Profile.MemberNumber
					};
					reportItems.Add(reportItem);
				}
			}

			return reportItems;
		}

		private static MemberAttendanceReportItem MapMemberAttendanceReportItem(SignInEntry signIn, IList<IdentityUser> signInUsers)
		{
			// Create the report item
			MemberAttendanceReportItem reportItem = new MemberAttendanceReportItem()
			{
				SignInType = signIn.SignInType,
				SignInTime = signIn.SignInTime.ToLocalTime()
			};

			// Get the user for the sign in entry
			IdentityUser user = signInUsers.FirstOrDefault(i => i.Id == signIn.UserId);
			if (user != null)
			{
				reportItem.FullName = user.FullName;
				reportItem.MemberNumber = user.Profile.MemberNumber;
			}

			// Get the sign in description
			switch (signIn.SignInType)
			{
				case SignInType.Operation:
					reportItem.Description = (signIn.OperationDetails != null ? signIn.OperationDetails.Description : "");
					break;

				case SignInType.Training:
					reportItem.Description = (signIn.TrainingDetails != null ? signIn.TrainingDetails.Description : "");
					break;

				case SignInType.Other:
					if (signIn.OtherDetails != null)
					{
						reportItem.Description = (signIn.OtherDetails.OtherType != OtherSignInType.Other ? signIn.OtherDetails.OtherType.GetEnumDescription() : String.Format("Other - {0}", signIn.OtherDetails.OtherDescription));
					}
					break;
			}

			// return the report item
			return reportItem;
		}
		

		#endregion

	}
}