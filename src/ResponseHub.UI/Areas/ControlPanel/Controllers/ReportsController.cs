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
using Enivate.ResponseHub.Common.Constants;

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
		private const int TrainingSessionVariance = 3;

		[Route]
		// GET: ControlPanel/DataExport
		public ActionResult Index()
		{
			return View();
		}

		#region Training report

		[Route("training-report")]
		public async Task<ActionResult> TrainingReport()
		{

			// Create the model
			TrainingReportFilterViewModel model = new TrainingReportFilterViewModel();

			// Load the users for the model
			IList<IdentityUser> users = await UnitService.GetUsersForUnit(GetControlPanelUnitId());
			foreach (IdentityUser user in users)
			{
				model.AvailableMembers.Add(new Tuple<Guid, string, string>(user.Id, user.FullName, user.Profile.MemberNumber));
			}

			return View(model);
		}

		[Route("training-report")]
		[HttpPost]
		public async Task<ActionResult> TrainingReport(TrainingReportFilterViewModel model)
		{
			
			// Load the users for the model
			IList<IdentityUser> users = await UnitService.GetUsersForUnit(GetControlPanelUnitId());
			model.AvailableMembers.Clear();
			foreach (IdentityUser user in users)
			{
				model.AvailableMembers.Add(new Tuple<Guid, string, string>(user.Id, user.FullName, user.Profile.MemberNumber));
			}

			// Get the list of jobs between the start and end dates
			DateTime dateFrom = model.DateFrom.Date;
			DateTime dateTo = new DateTime(model.DateTo.Year, model.DateTo.Month, model.DateTo.Day, 23, 59, 59);

			if (model.ReportFormat.ToLower() == "display")
			{
				TrainingReportViewModel reportViewModel = await GetTrainingReportModel(GetControlPanelUnitId(), dateFrom, dateTo, false, model.MemberId);
				reportViewModel.UseStandardLayout = true;
				return View("GenerateTrainingReportHtml", reportViewModel);
			}
			else if (model.ReportFormat.ToLower() == "pdf")
			{
				// Get the PDF bytes
				byte[] pdfBytes = await ReportService.GenerateTrainingReportPdfFile(GetControlPanelUnitId(), dateFrom, dateTo, model.MemberId, Request.Cookies);

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
			Guid? memberId = null;
			if (!String.IsNullOrEmpty(Request.QueryString["member_id"]))
			{
				memberId = new Guid(Request.QueryString["member_id"]);
			}
            bool canvasToImage = (Request.QueryString["canvas_to_image"] != null && Request.QueryString["canvas_to_image"] == "1");

			// Get the training report model
			TrainingReportViewModel model = await GetTrainingReportModel(unitId, dateFrom, dateTo, canvasToImage, memberId);

			return View(model);

		}

        #endregion

        #region Training Activity report

        [Route("training-activity-report")]
        public ActionResult TrainingActivityReport()
        {

            // Create the model
            TrainingActivityReportFilterViewModel model = new TrainingActivityReportFilterViewModel();

            return View(model);
        }

        [Route("training-activity-report")]
        [HttpPost]
        public async Task<ActionResult> TrainingActivityReport(TrainingActivityReportFilterViewModel model)
        {

            // Get the list of jobs between the start and end dates
            DateTime dateFrom = model.DateFrom.Date;
            DateTime dateTo = new DateTime(model.DateTo.Year, model.DateTo.Month, model.DateTo.Day, 23, 59, 59);

            if (model.ReportFormat.ToLower() == "display")
            {
                TrainingActivityReportViewModel reportViewModel = await GetTrainingActivityReportModel(GetControlPanelUnitId(), dateFrom, dateTo);
                reportViewModel.UseStandardLayout = true;
                return View("GenerateTrainingActivityReportHtml", reportViewModel);
            }
            else if (model.ReportFormat.ToLower() == "pdf")
            {
                // Get the PDF bytes
                byte[] pdfBytes = await ReportService.GenerateTrainingActivityReportPdfFile(GetControlPanelUnitId(), dateFrom, dateTo, Request.Cookies);

                FileContentResult result = new FileContentResult(pdfBytes, "application/pdf");
                result.FileDownloadName = String.Format("training-activity-report-{0}.pdf", DateTime.Now.ToString("yyyy-MM-dd"));
                return result;
            }
            else
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Bad request.");
            }
        }


        [Route("generate-training-activity-report-html")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GenerateTrainingActivityReportHtml()
        {

            // Get the parameters from the query string
            Guid unitId = new Guid(Request.QueryString["unit_id"]);
            DateTime dateFrom = DateTime.ParseExact(Request.QueryString["date_from"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
            DateTime dateTo = DateTime.ParseExact(Request.QueryString["date_to"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);

            // Get the training report model
            TrainingActivityReportViewModel model = await GetTrainingActivityReportModel(unitId, dateFrom, dateTo);

            return View(model);

        }

        #endregion

        #region Trainers report

        [Route("trainers-report")]
		public ActionResult TrainersReport()
		{
			return View();
		}

		[Route("trainers-report")]
		[HttpPost]
		public async Task<ActionResult> TrainersReport(ReportFilterViewModel model)
		{

			// Get the list of jobs between the start and end dates
			DateTime dateFrom = model.DateFrom.Date;
			DateTime dateTo = new DateTime(model.DateTo.Year, model.DateTo.Month, model.DateTo.Day, 23, 59, 59);

			if (model.ReportFormat.ToLower() == "display")
			{
				TrainersReportViewModel reportViewModel = await GetTrainersReportModel(GetControlPanelUnitId(), dateFrom, dateTo);
				reportViewModel.UseStandardLayout = true;
				return View("GenerateTrainersReportHtml", reportViewModel);
			}
			else if (model.ReportFormat.ToLower() == "pdf")
			{
				// Get the PDF bytes
				byte[] pdfBytes = await ReportService.GenerateTrainersReportPdfFile(GetControlPanelUnitId(), dateFrom, dateTo, Request.Cookies);

				FileContentResult result = new FileContentResult(pdfBytes, "application/pdf");
				result.FileDownloadName = String.Format("trainers-report-{0}.pdf", DateTime.Now.ToString("yyyy-MM-dd"));
				return result;
			}
			else
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, "Bad request.");
			}
		}

		[Route("generate-trainers-report-html")]
		[HttpGet]
		[AllowAnonymous]
		public async Task<ActionResult> GenerateTrainersReportHtml()
		{

			// Get the parameters from the query string
			Guid unitId = new Guid(Request.QueryString["unit_id"]);
			DateTime dateFrom = DateTime.ParseExact(Request.QueryString["date_from"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
			DateTime dateTo = DateTime.ParseExact(Request.QueryString["date_to"], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);

			// Get the training report model
			TrainersReportViewModel model = await GetTrainersReportModel(unitId, dateFrom, dateTo);

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
				selectedCapcodes.Select(i => i.CapcodeAddress),
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
				FinishDate = dateTo,
                UnitCapcodeAddress = unit.Capcode
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
		private async Task<TrainingReportViewModel> GetTrainingReportModel(Guid unitId, DateTime dateFrom, DateTime dateTo, bool canvasToImage, Guid? memberId)
		{
			// Get the training sessions
			IList<TrainingSession> trainingSessions = await TrainingService.GetTrainingSessionsForUnit(unitId, dateFrom, dateTo, memberId);
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
			sbChartData.Append("],\"datasets\": [{\"data\":[");
			for (int i = 0; i < aggregate.Count; i++)
			{
				KeyValuePair<string, int> item = aggregate.ElementAt(i);
				sbChartData.AppendFormat("{0}{1}", (i == 0 ? "" : ","), item.Value);
			}
			sbChartData.Append("],");
			sbChartData.AppendFormat("\"backgroundColor\":[{0}],\"borderColor\":[{1}],\"borderWidth\":1", ChartConstants.ChartFillColourSet, ChartConstants.ChartBorderColourSet);
			sbChartData.Append("}]}");

			StringBuilder sbChartOptionsJs = new StringBuilder();

			// Build the member reports
			IList<UnitTrainingMemberReportItem> unitMemberReports = new List<UnitTrainingMemberReportItem>();

			// Create the user full name variable
			string memberFullname = "";

			// If there is only a single member, then load only that report information
			if (memberId != null && memberId.Value != Guid.Empty)
			{

				// Get the user based on the member id
				IdentityUser user = unitMembers.FirstOrDefault(i => i.Id == memberId.Value);

				// If it's not null, map the user to the training report item
				if (user != null)
				{

					// Set the full name for later use
					memberFullname = user.FullName;

					// Map the aggregate training sessions
					MapUserToTrainingReportItem(trainingSessions, trainingSessionDays, trainingTypes, unitMemberReports, user);
				}

			}
			else
			{

				// Create the member report items
				foreach (IdentityUser user in unitMembers)
				{
					MapUserToTrainingReportItem(trainingSessions, trainingSessionDays, trainingTypes, unitMemberReports, user);
				}
			}
			
			// Generate the list of member training sessions
			List<TrainingSessionItem> memberTrainingSessions = new List<TrainingSessionItem>();

			// Loop through the training sessions
			foreach (TrainingSession session in trainingSessions)
			{
				memberTrainingSessions.Add(new TrainingSessionItem
				{
					Description = session.Description,
					Duration = session.Duration.ToString(),
					Id = session.Id,
					Name = session.Name,
					SessionDate = session.SessionDate.ToLocalTime(),
					TrainingType = String.Join(", ", session.TrainingTypes.Select(i => i.Name)),
                    EquipmentUsed = session.EquipmentUsed
				});
			}

            // Create the model
            TrainingReportViewModel model = new TrainingReportViewModel()
            {
                ChartDataJs = sbChartData.ToString(),
                ChartOptionsJs = sbChartOptionsJs.ToString(),
                MemberReports = unitMemberReports,
                DateFrom = dateFrom,
                DateTo = dateTo,
                CanvasToImage = canvasToImage,
				MemberId = memberId,
				MemberFullName = memberFullname,
				MemberSessions = memberTrainingSessions
			};

			// Get the training types
			foreach (TrainingType trainingType in trainingTypes)
			{
				model.TrainingTypes.Add(trainingType, trainingType.ShortName);
			}

			return model;
		}

		/// <summary>
		/// Maps a user to a specified training report item.
		/// </summary>
		/// <param name="trainingSessions"></param>
		/// <param name="trainingSessionDays"></param>
		/// <param name="trainingTypes"></param>
		/// <param name="unitMemberReports"></param>
		/// <param name="user"></param>
		private static void MapUserToTrainingReportItem(IList<TrainingSession> trainingSessions, int trainingSessionDays, IList<TrainingType> trainingTypes, IList<UnitTrainingMemberReportItem> unitMemberReports, IdentityUser user)
		{
			// Create the training item
			UnitTrainingMemberReportItem memberTrainingRecord = new UnitTrainingMemberReportItem()
			{
				Name = user.FullName
			};

			// Get the training session where the user is recorded as either a member or trainer
			IList<TrainingSession> userSessions = trainingSessions.Where(i => i.Members.Contains(user.Id) || i.Trainers.Contains(user.Id)).Distinct().ToList();

			// If there is more than one training session for the member, calculate some percentages.
			if (trainingSessions.Count > 0)
			{
				// Get the actual reported percentage of attendance
				memberTrainingRecord.AttendancePercent = (int)(((decimal)userSessions.Count / (decimal)trainingSessionDays) * 100);

				// Get the percentage of attendance varince by include an additional 3 sessions on 3 days to account for any variance in reporting
				memberTrainingRecord.AttendancePercentVariance = (int)(((decimal)(userSessions.Count + TrainingSessionVariance) / (decimal)(trainingSessionDays + TrainingSessionVariance)) * 100);
			}

			// Get the training types
			foreach (TrainingType trainingType in trainingTypes)
			{

				// Get the total amount of training sessions for each session
				memberTrainingRecord.TrainingSessions.Add(trainingType.ShortName, userSessions.Where(i => i.TrainingTypes.Contains(trainingType)).Count());
			}

			// Add the total row
			memberTrainingRecord.TrainingSessions.Add("Total", userSessions.Count);

			// Add to the list of members
			unitMemberReports.Add(memberTrainingRecord);
		}

        /// <summary>
		/// Gets the training report model to display to the page.
		/// </summary>
		/// <param name="unitId"></param>
		/// <param name="dateFrom"></param>
		/// <param name="dateTo"></param>
		/// <returns></returns>
		private async Task<TrainingActivityReportViewModel> GetTrainingActivityReportModel(Guid unitId, DateTime dateFrom, DateTime dateTo)
        {
            // Get the training sessions
            IList<TrainingSession> trainingSessions = await TrainingService.GetTrainingSessionsForUnit(unitId, dateFrom, dateTo, null);

            // Get the members for the unit
            IList<IdentityUser> unitMembers = await UnitService.GetUsersForUnit(unitId);

            // Get the training types.
            IList<TrainingType> trainingTypes = await TrainingService.GetAllTrainingTypes();
            
            // Generate the list of member training sessions
            List<TrainingActivityItem> trainingActivitySessions = new List<TrainingActivityItem>();

            // Loop through the training sessions
            foreach (TrainingSession session in trainingSessions)
            {
                // Create the training activity item
                TrainingActivityItem activity = new TrainingActivityItem
                {
                    Description = session.Description,
                    Duration = session.Duration.ToString(),
                    Id = session.Id,
                    Name = session.Name,
                    SessionDate = session.SessionDate.ToLocalTime(),
                    TrainingType = String.Join(", ", session.TrainingTypes.Select(i => i.Name)),
                    SessionType = session.SessionType.GetEnumDescription(),
                    EquipmentUsed = session.EquipmentUsed
                };

                // Map the members
                foreach(Guid id in session.Members)
                {
                    IdentityUser member = unitMembers.FirstOrDefault(i => i.Id == id);
                    activity.Members.Add(new TrainingAttendanceItem()
                    {
                        MemberId = id,
                        FirstName = member.FirstName,
                        Surname = member.Surname,
                        MemberNumber = member.Profile.MemberNumber
                    });
                }


                // Map the trainers
                foreach (Guid id in session.Trainers)
                {
                    IdentityUser member = unitMembers.FirstOrDefault(i => i.Id == id);
                    activity.Trainers.Add(new TrainingAttendanceItem()
                    {
                        MemberId = id,
                        FirstName = member.FirstName,
                        Surname = member.Surname,
                        MemberNumber = member.Profile.MemberNumber
                    });
                }

                trainingActivitySessions.Add(activity);
            }

            // Get the unit
            Unit unit = await UnitService.GetById(unitId);

            // Create the model
            TrainingActivityReportViewModel model = new TrainingActivityReportViewModel()
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                TrainingActivitySessions = trainingActivitySessions,
                UnitName = unit.Name
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
        private async Task<TrainersReportViewModel> GetTrainersReportModel(Guid unitId, DateTime dateFrom, DateTime dateTo)
		{
			// Get the training sessions
			IList<TrainingSession> trainingSessions = await TrainingService.GetTrainingSessionsForUnit(unitId, dateFrom, dateTo, null);
			int trainingSessionDays = trainingSessions.GroupBy(i => i.SessionDate.Date).Count();

			// Get the members for the unit
			IList<IdentityUser> unitMembers = await UnitService.GetUsersForUnit(unitId);

			// Get the training types.
			IList<TrainingType> trainingTypes = await TrainingService.GetAllTrainingTypes();
			
			// Build the member reports
			IList<UnitTrainerReportItem> unitMemberReports = new List<UnitTrainerReportItem>();

			// Create the member report items
			foreach (IdentityUser user in unitMembers)
			{
				// Create the training item
				UnitTrainerReportItem memberTrainingRecord = new UnitTrainerReportItem()
				{
					Name = user.FullName
				};

				// Get the training session where the user is recorded as either a member or trainer
				IList<TrainingSession> userSessions = trainingSessions.Where(i => i.Trainers.Contains(user.Id)).Distinct().ToList();

				// If there was no sessions the user trained in, then just continue to the next user, otherwise set the count of sessions trained.
				if (userSessions.Count == 0)
				{
					continue;
				}
				else
				{
					memberTrainingRecord.TotalSessionsTrained = userSessions.Count;
				}

				// Get the training types
				foreach (TrainingSession trainingSession in userSessions)
				{

					// Get the training types
					string trainingTypeNames = String.Join(", ", trainingSession.TrainingTypes.Select(i => i.Name));

					// Add the training session item
					memberTrainingRecord.TrainingSessions.Add(new TrainerSessionItem
					{
						SessionDate = trainingSession.SessionDate.ToLocalTime(),
						Description = trainingSession.Description,
						SessionTypes = trainingTypeNames,
						Name = trainingSession.Name
					});
				}

				// Add to the list of trainers if they have a training session
				unitMemberReports.Add(memberTrainingRecord);
			}

			// Create the model
			TrainersReportViewModel model = new TrainersReportViewModel()
			{
				TrainerReports = unitMemberReports,
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
			IList<TrainingSession> trainingSessions = await TrainingService.GetTrainingSessionsForUnit(unitId, dateFrom, dateTo, null);

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