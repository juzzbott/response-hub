using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.UI.Models.Api.Search;
using Enivate.ResponseHub.UI.Models.Search;
using Enivate.ResponseHub.Model.Groups;

using Microsoft.AspNet.Identity;
using System.Globalization;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.UI.Models.Messages;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;
using Enivate.ResponseHub.UI.Helpers;

namespace Enivate.ResponseHub.UI.Controllers.Api
{

	[RoutePrefix("api/search")]
	public class SearchController : BaseApiController
	{



		protected readonly ICapcodeService CapcodeService = ServiceLocator.Get<ICapcodeService>();
		protected readonly IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();

		[Route]
		[HttpPost]
		public async Task<SearchViewModel> PostSearch(SearchPostModel model)
		{

			DateTime dateFrom = DateTime.MinValue;
			DateTime dateTo = DateTime.Now;
			bool dateFromSet = false;
			bool dateToSet = false;

			// If there is a date from, set it
			if (!String.IsNullOrEmpty(model.DateFrom))
			{
				dateFromSet = DateTime.TryParseExact(model.DateFrom, "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dateFrom);
			}

			// If there is a date from, set it
			if (!String.IsNullOrEmpty(model.DateTo))
			{
				dateToSet = DateTime.TryParseExact(model.DateTo, "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dateTo);
			}

			// If there is a message type, set it
			MessageType messageTypes = MessageType.Job | MessageType.Message;
			if (model.MessageTypes.Contains(MessageType.Message))
			{
				messageTypes &= ~MessageType.Message;
			}
			if (model.MessageTypes.Contains(MessageType.Job))
			{
				messageTypes &= ~MessageType.Job;
			}

			// If the keywords are not null or empty, perform the search otherwise return null model
			if (!String.IsNullOrEmpty(model.Keywords))
			{

				// Trim any whitespace
				string keywords = model.Keywords.Trim();

				// Get the current user id
				Guid userId = new Guid(User.Identity.GetUserId());

				// Get the capcodes for the current user
				IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(userId);

				// Perform the search
				PagedResultSet<JobMessage> results = await JobMessageService.FindByKeyword(keywords, capcodes.Select(i => i.CapcodeAddress), messageTypes, dateFrom, dateTo, 50, model.Skip, true);

				// Create the job message list view model
				JobMessageListViewModel resultsModel = await CreateJobMessageListModel(capcodes, results.Items);

				// Create the model
				SearchViewModel mappedResults = new SearchViewModel
				{
					SearchKeywords = keywords,
					Results = resultsModel.Messages,
					TotalResults = results.TotalResults,
					DateFrom = (dateFromSet ? dateFrom : (DateTime?)null),
					DateTo = (dateToSet ? dateTo : (DateTime?)null),
					MessageTypes = messageTypes
				};

				return mappedResults;

			}

			return null;

		}

		#region Helpers

		/// <summary>
		/// Creates the JobMessageListViewModel object from the list of messages and capcodes.
		/// </summary>
		/// <param name="capcodes"></param>
		/// <param name="jobMessages"></param>
		/// <returns></returns>
		public async Task<JobMessageListViewModel> CreateJobMessageListModel(IList<Capcode> capcodes, IList<JobMessage> jobMessages)
		{
			// Create the list of job message view models
			IList<JobMessageViewModel> jobMessageViewModels = new List<JobMessageViewModel>();
			foreach (JobMessage jobMessage in jobMessages)
			{

				// Get the capcode for the job message
				Capcode capcode = capcodes.FirstOrDefault(i => i.CapcodeAddress == jobMessage.Capcode);

				// Map the view model and add to the list
				jobMessageViewModels.Add(await MapJobMessageToViewModel(jobMessage, capcode.FormattedName()));

			}

			// Create the model object
			JobMessageListViewModel model = new JobMessageListViewModel()
			{
				Messages = jobMessageViewModels,
				UserCapcodes = capcodes
			};
			return model;
		}

		public async Task<JobMessageViewModel> MapJobMessageToViewModel(JobMessage job, string capcodeGroupName)
		{


			// Map the job notes to the list of job notes view models
			IList<JobNoteViewModel> jobNotesModels = await JobMessageModelHelper.MapJobNotesToViewModel(job.Notes, UserService);

			JobMessageViewModel model = new JobMessageViewModel()
			{
				Capcode = job.Capcode,
				CapcodeGroupName = capcodeGroupName,
				Id = job.Id,
				JobNumber = job.JobNumber,
				Location = job.Location,
				MessageBody = job.MessageContent,
				Notes = jobNotesModels,
				Priority = job.Priority,
				Timestamp = job.Timestamp.ToLocalTime()
			};

			// Set the on route, on scene, job clear values
			model.OnRoute = await GetProgressModel(job, MessageProgressType.OnRoute);
			model.OnScene = await GetProgressModel(job, MessageProgressType.OnScene);
			model.JobClear = await GetProgressModel(job, MessageProgressType.JobClear);
			model.Cancelled = await GetProgressModel(job, MessageProgressType.Cancelled);

			// return the mapped job view model
			return model;

		}

		/// <summary>
		/// Gets the progress model for the specific progress type, if it exists. 
		/// </summary>
		/// <param name="job">The job to get the progress from. </param>
		/// <param name="progressType">The progress type to get.</param>
		/// <returns></returns>
		public static async Task<MessageProgressViewModel> GetProgressModel(JobMessage job, MessageProgressType progressType)
		{
			MessageProgress progress = job.ProgressUpdates.FirstOrDefault(i => i.ProgressType == progressType);
			if (progress != null)
			{
				MessageProgressViewModel progressModel = new MessageProgressViewModel()
				{
					Timestamp = progress.Timestamp.ToLocalTime(),
					UserId = progress.UserId,
					ProgressType = progress.ProgressType
				};

				// Get the user who updated the progress.
				IdentityUser progressUser = await ServiceLocator.Get<IUserService>().FindByIdAsync(progress.UserId);
				if (progressUser != null)
				{
					progressModel.UserFullName = progressUser.FullName;
				}

				return progressModel;
			}
			else
			{
				return null;
			}
		}

		#endregion

	}
}
