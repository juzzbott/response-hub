using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

using Microsoft.AspNet.Identity;
using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Messages;
using System.Threading.Tasks;
using Enivate.ResponseHub.UI.Models.Api.Messages;
using Enivate.ResponseHub.Model.Identity.Interface;
using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Controllers.Api
{

	[RoutePrefix("api/job-messages")]
    public class JobMessageController : BaseApiController
    {

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
				JobNote note = await JobMessageService.AddNoteToJobMessage(id, jobNote.Body, jobNote.IsWordBack, UserId);

				return note;

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Error adding job note to job message. Message: {0}", ex.Message), ex);
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
			}

		}

		[Route("{id:guid}/progress")]
		[HttpPost]
		public async Task<MessageProgressResponseModel> PostProgress(Guid id, [FromBody] MessageProgressType progressType)
		{

			try
			{
				
				// Get the identity user for the current user
				IdentityUser user = await GetCurrentUser();

				// If the user cannot be found, return null
				if (user == null)
				{
					return null;
				}

				// Create the progress object and return it
				MessageProgress progress = await JobMessageService.AddProgress(id, UserId, progressType);
				return new MessageProgressResponseModel()
				{
					Timestamp = progress.Timestamp,
					UserId = UserId,
					UserFullName = user.FullName,
					Success = true
				};

			} 
			catch(Exception ex)
			{
				// If the exception messge relates to not being able to set the progress update because a progress type already exists, treat as warning
				if (ex.Message.StartsWith("", StringComparison.CurrentCultureIgnoreCase))
				{
					await Log.Warn(String.Format("Error adding progress to job message. Message: {0}", ex.Message));
					return new MessageProgressResponseModel()
					{
						Success = false,
						ErrorMessage = ex.Message
					};
				}
				else
				{
					await Log.Error(String.Format("Error adding progress to job message. Message: {0}", ex.Message), ex);
					throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
				}
			}



		}

	}
}
