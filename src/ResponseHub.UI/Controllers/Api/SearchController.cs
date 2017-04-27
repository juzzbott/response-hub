using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.UI.Models.Api.Search;
using Enivate.ResponseHub.UI.Models.Search;
using Enivate.ResponseHub.Model.Units;

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
				JobMessageListViewModel resultsModel = CreateJobMessageListModel(capcodes, results.Items);

				// Create the model
				SearchViewModel mappedResults = new SearchViewModel
				{
					SearchKeywords = keywords,
					Results = resultsModel.JobMessages,
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
		public JobMessageListViewModel CreateJobMessageListModel(IList<Capcode> capcodes, IList<JobMessage> jobMessages)
		{
			// Create the list of job message view models
			IList<JobMessageListItemViewModel> jobMessageViewModels = new List<JobMessageListItemViewModel>();
			foreach (JobMessage jobMessage in jobMessages)
			{

				// Get the capcode for the job message
				Capcode capcode = capcodes.FirstOrDefault(i => i.CapcodeAddress == jobMessage.Capcode);

				// Map the view model and add to the list
				JobMessageListItemViewModel jobListItem = JobMessageListItemViewModel.FromJobMessage(jobMessage, capcode, null);
				if (jobListItem != null)
				{
					jobMessageViewModels.Add(jobListItem);
				}

			}

			// Create the model object
			JobMessageListViewModel model = new JobMessageListViewModel()
			{
				JobMessages = jobMessageViewModels
			};
			return model;
		}

		#endregion

	}
}
