using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;

using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.UI.Models.Search;

namespace Enivate.ResponseHub.UI.Controllers
{
    public class SearchController : BaseJobsMessagesController
	{
		
        // GET: Search
        public async Task<ActionResult> Index()
        {

			// Get the search keywords from the query string
			string keywords = Request.QueryString["q"];

			DateTime dateFrom = DateTime.MinValue;
			DateTime dateTo = DateTime.Now;
			bool dateFromSet = false;
			bool dateToSet = false;

			// If there is a date from, set it
			if (!String.IsNullOrEmpty(Request.QueryString["date_from"]))
			{
				dateFromSet = DateTime.TryParseExact(Request.QueryString["date_from"], "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dateFrom);
			}

			// If there is a date from, set it
			if (!String.IsNullOrEmpty(Request.QueryString["date_to"]))
			{
				dateToSet = DateTime.TryParseExact(Request.QueryString["date_to"], "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dateTo);
			}

			// If there is a message type, set it
			MessageType messageTypes = MessageType.Job | MessageType.Message;
			if (String.IsNullOrEmpty(Request.QueryString["messagetype_message"]) || Request.QueryString["messagetype_message"] != "1")
			{
				messageTypes &= ~MessageType.Message;
			}
			if (String.IsNullOrEmpty(Request.QueryString["messagetype_job"]) || Request.QueryString["messagetype_job"] != "1")
			{
				messageTypes &= ~MessageType.Job;
			}

			// If the keywords are not null or empty, perform the search otherwise return null model
			if (!String.IsNullOrEmpty(keywords))
			{

				// Trim any whitespace
				keywords = keywords.Trim();

				// Get the current user id
				Guid userId = new Guid(User.Identity.GetUserId());

				// Get the capcodes for the current user
				IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(userId);

				// Perform the search
				PagedResultSet<JobMessage> results = await JobMessageService.FindByKeyword(keywords, capcodes.Select(i => i.CapcodeAddress), messageTypes, dateFrom, dateTo, 50, 0, true);

				// Create the model
				SearchViewModel model = new SearchViewModel
				{
					SearchKeywords = keywords,
					Results = await CreateJobMessageListModel(capcodes, results.Items),
					TotalResults = results.TotalResults,
					DateFrom = (dateFromSet ? dateFrom : (DateTime?)null),
					DateTo = (dateToSet ? dateTo : (DateTime?)null),
					MessageTypes = messageTypes
				};

				return View(model);

			}
			else
			{
				return View();
			}
        }
    }
}