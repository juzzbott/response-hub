using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.AspNet.Identity;
using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.UI.Models.Api.Messages;
using Enivate.ResponseHub.UI.Models.Messages;

namespace Enivate.ResponseHub.UI.Controllers.Api
{

	[RoutePrefix("api/job-messages")]
    public class JobMessageController : BaseApiController
    {
		protected IJobMessageService JobMessageService
		{
			get
			{
				return ServiceLocator.Get<IJobMessageService>();
			}
		}

		protected ICapcodeService CapcodeService
		{
			get
			{
				return ServiceLocator.Get<ICapcodeService>();
			}
		}

		[Route]
		[HttpGet]
		public async Task<IList<JobMessageViewModel>> Get()
		{

			// Get the capcodes for the user.
			IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(UserId);

			// Get the job messages
			IList<JobMessage> jobMessages = await JobMessageService.GetMostRecent(capcodes, MessageType.Job, 30);

			// return the mapped view models
			IList<JobMessageViewModel> models = new List<JobMessageViewModel>();
			foreach(JobMessage message in jobMessages)
			{

				// Get the capcode
				string capcodeGroupName = capcodes.FirstOrDefault(i => i.CapcodeAddress == message.Capcode).FormattedName();

				// Add the mapped job message view model
				models.Add(await BaseJobsMessagesController.MapJobMessageToViewModel(message, capcodeGroupName));
			}

			// return the mapped models
			return models;

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
			
			// Get the identity user for the current user
			IdentityUser user = await GetCurrentUser();

			// If the user cannot be found, return null
			if (user == null)
			{
				return null;
			}

			try
			{

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
				if (ex.Message.StartsWith("The job message already contains a progress update", StringComparison.CurrentCultureIgnoreCase))
				{
					await Log.Warn(String.Format("Error adding progress to job message. Message: {0}", ex.Message));
					return new MessageProgressResponseModel()
					{
						Success = false,
						ErrorMessage = ex.Message
					};
				}
				else if (ex.Message.StartsWith("Cannot add progress to cancelled job", StringComparison.CurrentCultureIgnoreCase))
				{
					await Log.Warn(String.Format("Cannot update progress on cancelled job.", ex.Message));
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

		[Route("latest-from-last-id")]
		[HttpPost]
		public async Task<IList<JobMessageViewModel>> GetLatestMessagesFromLast(PostGetLatestFromLastModel model)
		{

			try
			{

				// Get the capcodes for the current user
				IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(UserId);
				IList<string> capcodeStrings = capcodes.Select(i => i.CapcodeAddress).ToList();

				// Get the latest messages
				IList<JobMessage> latestMessages = await JobMessageService.GetLatestFromLastMessage(model.LastMessageId, capcodeStrings, model.MessageType);

				// Create the list of job message view models
				IList<JobMessageViewModel> latestMessagesModels = new List<JobMessageViewModel>();

				// Map the job messages to the JobMesageViewModel type
				if (latestMessages != null && latestMessages.Count > 0)
				{
					
					// Iterate through each message and add to the list
					foreach(JobMessage message in latestMessages)
					{
						// Get the capcode group name from the list of capcodes
						Capcode messageCapcode = capcodes.FirstOrDefault(i => i.CapcodeAddress == message.Capcode);
						string capcodeGroupName = messageCapcode?.FormattedName();

						// If there was no group capcode name, just set to unknown
						if (String.IsNullOrEmpty(capcodeGroupName))
						{
							capcodeGroupName = "Unknown";
						}

						// Map to the JobMessageViewModel
						latestMessagesModels.Add(await BaseJobsMessagesController.MapJobMessageToViewModel(message, capcodeGroupName));
					}

				}

				// return the latest messages models
				return latestMessagesModels;

			}
			catch (Exception ex)
			{
				await Log.Error("Unable to get latest messages from last id from the api. Message: " + ex.Message, ex);
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
			}

		}

	}
}
