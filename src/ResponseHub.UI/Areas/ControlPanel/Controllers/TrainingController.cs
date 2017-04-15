using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.UI.Filters;
using Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Training;
using Enivate.ResponseHub.Model.Training;
using Enivate.ResponseHub.Model.Training.Interface;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Common.Extensions;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Controllers
{

	[RouteArea("ControlPanel", AreaPrefix = "control-panel")]
	[RoutePrefix("training")]
	[ClaimsAuthorize(Roles = RoleTypes.UnitAdministrator)]
	public class TrainingController : BaseControlPanelController
    {

		public ITrainingService TrainingService = ServiceLocator.Get<ITrainingService>();

		// GET: ControlPanel/Training
		[Route]
        public async Task<ActionResult> Index()
		{

			// Create the model
			TrainingHomeViewModel model = new TrainingHomeViewModel();
			model.TrainingSessions = await TrainingService.GetTrainingSessionsForUnit(GetControlPanelUnitId());

			// Get the aggregate chart data
			IDictionary<string, int> aggregate = new Dictionary<string, int>();
			IList<TrainingType> trainingTypes = await TrainingService.GetAllTrainingTypes();
			foreach (TrainingType trainingType in trainingTypes)
			{
				aggregate.Add(trainingType.ShortName, model.TrainingSessions.Count(i => i.TrainingTypes.Contains(trainingType)));
			}

			// Build the chart data
			StringBuilder sbChartData = new StringBuilder();
			sbChartData.Append("{\"labels\": [");
			for(int i = 0; i < aggregate.Count; i++)
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

			// Set the json data
			model.TrainingOverviewChartData = sbChartData.ToString();

			return View(model);
		}

		[Route("add")]
		public async Task<ActionResult> Add()
		{
			AddEditTrainingSessionViewModel model = new AddEditTrainingSessionViewModel();

			// Set the available training types
			model.AvailableTrainingTypes = await TrainingService.GetAllTrainingTypes();
			
			// Load the users for the model
			IList<IdentityUser> users = await UnitService.GetUsersForUnit(GetControlPanelUnitId());
			foreach(IdentityUser user in users)
			{
				model.AvailableUsers.Add(new Tuple<Guid, string, string>(user.Id, user.FullName, user.Profile.MemberNumber));
			}

			return View("AddEdit", model);
		}

		[Route("add")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Add(AddEditTrainingSessionViewModel model)
		{

			try
			{

				// Get the training types
				IList<TrainingType> trainingTypes = await TrainingService.GetAllTrainingTypes();

				// Set the available training types
				model.AvailableTrainingTypes = trainingTypes;

				// Load the users for the model
				IList<IdentityUser> users = await UnitService.GetUsersForUnit(GetControlPanelUnitId());
				model.AvailableUsers.Clear();
				foreach (IdentityUser user in users)
				{
					model.AvailableUsers.Add(new Tuple<Guid, string, string>(user.Id, user.FullName, user.Profile.MemberNumber));
				}

				// If the model is not valid, return
				if (!ModelState.IsValid)
				{
					return View("AddEdit", model);
				}

				// Get the list of guids from the selected training types
				IList<Guid> trainingTypeIds = model.TrainingTypes.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(i => new Guid(i)).ToList();

				// Create the training session
				TrainingSession session = new TrainingSession()
				{
					Created = DateTime.UtcNow,
					UnitId = GetControlPanelUnitId(),
					SessionDate = model.SessionDate.ToUniversalTime(),
					TrainingTypes = trainingTypes.Where(i => trainingTypeIds.Contains(i.Id)).ToList(),
					Name = model.Name,
					Description = model.Description,
					SessionType = model.SessionType,
					Duration = model.Duration
				};

				// Map the members and trainers
				MapMembersAndTrainersToSession(model, ref session);

				// Create the training session in the database
				await TrainingService.CreateTrainingSession(session);

				// Redirect to the the training screen.
				return new RedirectResult("/control-panel/training");
				
			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Unable to save training session. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "There was a system error saving the training session.");
				return View("AddEdit", model);
			}
		}

		[Route("session/{id:guid}")]
		public async Task<ActionResult> View(Guid id)
		{

			// Get the training session by the id
			TrainingSession session = await TrainingService.GetTrainingSessionById(id);

			// If null, throw 404
			if (session == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "Content not found.");
			}

			// If the training session is not the units current session, 403 forbidden
			if (session.UnitId != GetControlPanelUnitId())
			{
				throw new HttpException((int)HttpStatusCode.Forbidden, "Forbidden.");
			}

			// Create the model
			ViewTrainingSessionViewModel model = new ViewTrainingSessionViewModel()
			{
				Id = session.Id,
				Created = session.Created,
				Description = session.Description,
				Name = session.Name,
				SessionDate = session.SessionDate,
				SessionType = session.SessionType,
				TrainingTypes = session.TrainingTypes,
				Duration = session.Duration
			};			

			// Load the users for the model
			IList<IdentityUser> users = await UnitService.GetUsersForUnit(GetControlPanelUnitId());

			// Add the members trained
			foreach (Guid userId in session.Members)
			{
				// Get the user from the list of users
				IdentityUser user = users.FirstOrDefault(i => i.Id == userId);
				
				// If the user is null, just continue to the next
				if (user == null)
				{
					continue;
				}

				model.Members.Add(new Tuple<Guid, string, string>(user.Id, user.FullName, user.Profile.MemberNumber));
			}

			// Add the trainers
			foreach (Guid userId in session.Trainers)
			{
				// Get the user from the list of users
				IdentityUser user = users.FirstOrDefault(i => i.Id == userId);

				// If the user is null, just continue to the next
				if (user == null)
				{
					continue;
				}

				model.Trainers.Add(new Tuple<Guid, string, string>(user.Id, user.FullName, user.Profile.MemberNumber));
			}

			// Determine the percent of members training for this session
			if (users.Count > 0)
			{
				model.MemberPercentTrained = (int)(((decimal)session.Members.Count / (decimal)users.Count) * 100);
			}

			return View(model);
		}

		[Route("session/{id:guid}/edit")]
		public async Task<ActionResult> Edit(Guid id)
		{

			// Get the training session by the id
			TrainingSession session = await TrainingService.GetTrainingSessionById(id);

			// If null, throw 404
			if (session == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "Content not found.");
			}

			// If the training session is not the units current session, 403 forbidden
			if (session.UnitId != GetControlPanelUnitId())
			{
				throw new HttpException((int)HttpStatusCode.Forbidden, "Forbidden.");
			}

			// Create the model
			AddEditTrainingSessionViewModel model = new AddEditTrainingSessionViewModel()
			{
				Name = session.Name,
				Description = session.Description,
				SessionDate = session.SessionDate,
				SessionType = session.SessionType,
				TrainingTypes = String.Format("{0}|", String.Join("|", session.TrainingTypes.Select(i => i.Id))),
				Duration = session.Duration,
				SelectedMembers = String.Format("{0}|", String.Join("|", session.Members)),
				SelectedTrainers = String.Format("{0}|", String.Join("|", session.Trainers))
			};

			// Set the available training types
			model.AvailableTrainingTypes = await TrainingService.GetAllTrainingTypes();

			// Load the users for the model
			IList<IdentityUser> users = await UnitService.GetUsersForUnit(GetControlPanelUnitId());
			foreach (IdentityUser user in users)
			{
				model.AvailableUsers.Add(new Tuple<Guid, string, string>(user.Id, user.FullName, user.Profile.MemberNumber));
			}

			return View("AddEdit", model);
		}

		[Route("session/{id:guid}/edit")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit(Guid id, AddEditTrainingSessionViewModel model)
		{
			// Get the training session by the id
			TrainingSession session = await TrainingService.GetTrainingSessionById(id);

			// If null, throw 404
			if (session == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "Content not found.");
			}

			// If the training session is not the units current session, 403 forbidden
			if (session.UnitId != GetControlPanelUnitId())
			{
				throw new HttpException((int)HttpStatusCode.Forbidden, "Forbidden.");
			}

			try
			{

				IList<TrainingType> trainingTypes = await TrainingService.GetAllTrainingTypes();

				// Set the available training types
				model.AvailableTrainingTypes = trainingTypes;

				// Load the users for the model
				IList<IdentityUser> users = await UnitService.GetUsersForUnit(GetControlPanelUnitId());
				model.AvailableUsers.Clear();
				foreach (IdentityUser user in users)
				{
					model.AvailableUsers.Add(new Tuple<Guid, string, string>(user.Id, user.FullName, user.Profile.MemberNumber));
				}

				// If the model is not valid, return
				if (!ModelState.IsValid)
				{
					return View("AddEdit", model);
				}

				// Get the list of guids from the selected training types
				IList<Guid> trainingTypeIds = model.TrainingTypes.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(i => new Guid(i)).ToList();

				// Set the session details
				session.SessionDate = model.SessionDate.ToUniversalTime();
				session.TrainingTypes = trainingTypes.Where(i => trainingTypeIds.Contains(i.Id)).ToList();
				session.Name = model.Name;
				session.Description = model.Description;
				session.SessionType = model.SessionType;
				session.Duration = model.Duration;

				// Map the members and trainers
				MapMembersAndTrainersToSession(model, ref session);

				// Save the session
				await TrainingService.SaveTrainingSession(session);

				// redirect back to the view training session screen
				return new RedirectResult(String.Format("/control-panel/training/session/{0}", session.Id));

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Unable to save training session. Message: {0}", ex.Message), ex);
				ModelState.AddModelError("", "There was a system error saving the training session.");
				return View("AddEdit", model);
			}

		}

		#region Helpers
		
		/// <summary>
		/// Maps the members and the trainers to the session from the view model.
		/// </summary>
		/// <param name="model">The view model containing the members and trainers.</param>
		/// <param name="session">The session object to map the members and trainers to.</param>
		private static void MapMembersAndTrainersToSession(AddEditTrainingSessionViewModel model, ref TrainingSession session)
		{

			// Clear the lists first
			session.Members.Clear();
			session.Trainers.Clear();

			// Add the users to the training session
			foreach (string userId in model.SelectedMembers.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
			{
				// If the string is empty, continue
				if (String.IsNullOrEmpty(userId))
				{
					continue;
				}

				session.Members.Add(new Guid(userId));
			}

			// Add the users to the training session
			foreach (string userId in model.SelectedTrainers.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
			{
				// If the string is empty, continue
				if (String.IsNullOrEmpty(userId))
				{
					continue;
				}

				session.Trainers.Add(new Guid(userId));
			}
		}

		#endregion

	}
}