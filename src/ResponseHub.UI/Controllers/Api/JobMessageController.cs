using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Messages;
using System.Threading.Tasks;
using Enivate.ResponseHub.UI.Models.Api.Messages;

namespace Enivate.ResponseHub.UI.Controllers.Api
{

	[RoutePrefix("api/job-messages")]
    public class JobMessageController : ApiController
    {

		private ILogger _log;
		protected ILogger Log
		{
			get
			{
				return _log ?? (_log = UnityConfiguration.Container.Resolve<ILogger>());
			}
		}

		private IJobMessageService _jobMessageService;
		protected IJobMessageService JobMessageService
		{
			get
			{
				return _jobMessageService ?? (_jobMessageService = UnityConfiguration.Container.Resolve<IJobMessageService>());
			}
		}

		[Route]
		[HttpPost]
		public async Task<bool> Post(IList<JobMessage> jobMessages)
		{

			// Get the authHeader
			AuthenticationHeaderValue authHeader = Request.Headers.Authorization;

			string apiKey = ConfigurationManager.AppSettings["ResponseHubService.ApiKey"];

			// If the api key is null or empty, log error message and return not authorized
			if (String.IsNullOrWhiteSpace(apiKey))
			{
				await Log.Error("The ResponseHub service API key is invalid.");
				throw new HttpResponseException(HttpStatusCode.Unauthorized);
			}

			// If there is no auth header, or it's no of type APIKEY with matching Api key, then throw not authorized.
			if (authHeader == null || !authHeader.Scheme.Equals("APIKEY", StringComparison.CurrentCultureIgnoreCase) || !authHeader.Parameter.Equals(apiKey))
			{
				throw new HttpResponseException(HttpStatusCode.Unauthorized);
			}

			try
			{

				// Save the pager message
				await JobMessageService.AddMessages(jobMessages);

				// return the last message sha
				return true;

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Error adding the job messages to the database from the api. Message: {0}", ex.Message), ex);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}
		}

		[Route("{id:guid}/notes")]
		[HttpPost]
		public async Task<JobNote> PostNote(Guid id, PostJobNoteModel jobNote)
		{

			try
			{
				// Create the job note and return it
				JobNote note = await JobMessageService.AddNoteToJobMessage(id, jobNote.Body, jobNote.IsWordBack, Guid.Empty);

				return note;

			}
			catch (Exception ex)
			{
				// TODO: Logging
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
			}

		}

	}
}
