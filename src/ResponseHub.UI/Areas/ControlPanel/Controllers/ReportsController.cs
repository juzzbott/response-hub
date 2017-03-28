﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Reports.Training;
using Enivate.ResponseHub.Common.Extensions;
using System.Text;
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

			// Get the PDF bytes
			// HACK: FixThis
			byte[] pdfBytes = await ReportService.GenerateTrainingReportPdfFile(new Guid("2E54F8EA-25E6-40C5-A64B-7950D84A059C"), dateFrom, dateTo);

			FileContentResult result = new FileContentResult(pdfBytes, "application/pdf");
			result.FileDownloadName = String.Format("training-report-{0}.pdf", DateTime.Now.ToString("yyyy-MM-dd"));
			return result;
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

			// Get the training sign-ins for the group
			IList<SignInEntry> signIns = await SignInService.GetSignInsForGroup(groupId, dateFrom, dateTo, SignInType.Training);

			// Count the different training types
			IList<GroupTrainingReportItem> trainingReportItems = new List<GroupTrainingReportItem>();
			foreach(SignInEntry signIn in signIns)
			{
				// If the list doesn't contain a training report item with the current date and type, add it
				if (!trainingReportItems.Any(i => i.TrainingType == ((TrainingActivity)signIn.ActivityDetails).TrainingType && i.TrainingDate == signIn.SignInTime.Date))
				{
					trainingReportItems.Add(new GroupTrainingReportItem() { TrainingDate = signIn.SignInTime.Date, TrainingType = ((TrainingActivity)signIn.ActivityDetails).TrainingType });
				}
			}

			// Count the different training types
			IDictionary<TrainingType, int> trainingTypeAggregate = new Dictionary<TrainingType, int>();
			foreach (TrainingType trainingType in Enum.GetValues(typeof(TrainingType)))
			{
				trainingTypeAggregate.Add(trainingType, trainingReportItems.Count(i => i.TrainingType == trainingType));
			}


			// Loop through the aggregates and create the list of values
			IList<GroupTrainingGraphItem> graphItems = new List<GroupTrainingGraphItem>();
			foreach(KeyValuePair<TrainingType, int> aggregate in trainingTypeAggregate)
			{
				switch (aggregate.Key)
				{
					case TrainingType.General:
						graphItems.Add(new GroupTrainingGraphItem() {
							Label = aggregate.Key.GetEnumDescription(),
							Value = aggregate.Value,
							Colour = "#27718F",
							HoverColour = "#0E5876"
						}
						);
						break;
					case TrainingType.GeneralRescue:
						graphItems.Add(new GroupTrainingGraphItem()
						{
							Label = aggregate.Key.GetEnumDescription(),
							Value = aggregate.Value,
							Colour = "#1F8F4A",
							HoverColour = "#067631"
						}
						);
						break;
					case TrainingType.LandSearch:
						graphItems.Add(new GroupTrainingGraphItem()
						{
							Label = aggregate.Key.GetEnumDescription(),
							Value = aggregate.Value,
							Colour = "#8C25A6",
							HoverColour = "#730C8D"
						}
						);
						break;
					case TrainingType.RoadRescue:
						graphItems.Add(new GroupTrainingGraphItem()
						{
							Label = aggregate.Key.GetEnumDescription(),
							Value = aggregate.Value,
							Colour = "#BF5717",
							HoverColour = "#A63E00"
						}
						);
						break;
					case TrainingType.Other:
						graphItems.Add(new GroupTrainingGraphItem()
						{
							Label = aggregate.Key.GetEnumDescription(),
							Value = aggregate.Value,
							Colour = "#C40600",
							HoverColour = "#AB0000"
						}
						);
						break;
				}
			}

			// Build the dataset js
			StringBuilder sbJsDataset = new StringBuilder();
			sbJsDataset.Append("{ labels: [");
			for (int i = 0; i < graphItems.Count; i++)
			{
				sbJsDataset.AppendFormat("{0}\"{1}\"", (i != 0 ? "," : ""), graphItems[i].Label);
			}
			sbJsDataset.Append("], datasets: [{ data: [");
			for (int i = 0; i < graphItems.Count; i++)
			{
				sbJsDataset.AppendFormat("{0}{1}", (i != 0 ? "," : ""), graphItems[i].Value);
			}
			sbJsDataset.Append("], backgroundColor: [");
			for (int i = 0; i < graphItems.Count; i++)
			{
				sbJsDataset.AppendFormat("{0}\"{1}\"", (i != 0 ? "," : ""), graphItems[i].Colour);
			}
			sbJsDataset.Append("], hoverBackgroundColor: [");
			for (int i = 0; i < graphItems.Count; i++)
			{
				sbJsDataset.AppendFormat("{0}\"{1}\"", (i != 0 ? "," : ""), graphItems[i].HoverColour);
			}
			sbJsDataset.Append("]}]}");

			// Get the members for the group
			IList<IdentityUser> groupMembers = await GroupService.GetUsersForGroup(groupId);

			// Create the member report items
			IList<GroupTrainingMemberReportItem> memberTraining = new List<GroupTrainingMemberReportItem>();
			foreach(IdentityUser user in groupMembers)
			{
				// Create the training item
				GroupTrainingMemberReportItem memberTrainingRecord = new GroupTrainingMemberReportItem()
				{
					Name = user.FullName
				};

				// Get the training types
				foreach (TrainingType trainingType in Enum.GetValues(typeof(TrainingType)))
				{
					// Get the total amount of training sessions for each session
					memberTrainingRecord.TrainingSessions.Add(trainingType, 
						signIns.Where(i => i.UserId == user.Id && ((TrainingActivity)i.ActivityDetails).TrainingType == trainingType).Count());
					memberTrainingRecord.TrainingDates.Add(trainingType,
						signIns.Where(i => i.UserId == user.Id && ((TrainingActivity)i.ActivityDetails).TrainingType == trainingType).Select(i => i.SignInTime).ToList());
				}

				// Add to the list of members
				memberTraining.Add(memberTrainingRecord);
			}

			// Create the model
			TrainingReportViewModel model = new TrainingReportViewModel()
			{
				ChartDataJs = sbJsDataset.ToString(),
				MemberReports = memberTraining,
				DateFrom = dateFrom,
				DateTo = dateTo
			};

			// Get the training types
			foreach (TrainingType trainingType in Enum.GetValues(typeof(TrainingType)))
			{
				model.TrainingTypes.Add(trainingType, trainingType.GetEnumDescription());
			}

			return View(model);

		}

	}
}