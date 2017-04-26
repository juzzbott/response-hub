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
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.UI.Models.Api.Messages;
using Enivate.ResponseHub.UI.Models.Messages;
using Enivate.ResponseHub.UI.Helpers;
using Enivate.ResponseHub.Model.SignIn;
using Enivate.ResponseHub.Model.SignIn.Interface;
using System.Globalization;
using Enivate.ResponseHub.ApplicationServices.Wrappers;
using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.UI.Controllers.Api
{

	[RoutePrefix("api/job-messages")]
    public class JobMessageController : BaseApiController
    {
		protected readonly IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();
		protected readonly ICapcodeService CapcodeService = ServiceLocator.Get<ICapcodeService>();
		protected readonly ISignInEntryService SignInEntryService = ServiceLocator.Get<ISignInEntryService>();

		[Route]
		[HttpGet]
		public async Task<IList<JobMessageListItemViewModel>> Get()
		{

			// Get the capcodes for the user.
			IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(UserId);

			// Store the skip and count values
			int count = 50;
			int skip = 0;
			MessageType messageType = MessageType.Job;

			// Create the list of job messages
			IList<JobMessage> jobMessages;

			// Get the query string
			IEnumerable<KeyValuePair<string, string>> qs = ControllerContext.Request.GetQueryNameValuePairs();

			// if there is a skip value, then get it from the query string
			if (qs.Any(i => i.Key.ToLower() == "skip"))
			{
				Int32.TryParse(qs.FirstOrDefault(i => i.Key.ToLower() == "skip").Value, out skip);
			}

			// Check to see if the job type is overridden in the query string
			if (qs.Any(i => i.Key.ToLower() == "msg_type"))
			{
				if (qs.FirstOrDefault(i => i.Key.ToLower() == "msg_type").Value == "job")
				{
					messageType = MessageType.Job;
				}
				else if (qs.FirstOrDefault(i => i.Key.ToLower() == "msg_type").Value == "message")
				{
					messageType = MessageType.Message;
				}
			}

			// If there is no date values set, then just get the most recent
			if (qs.Any(i => i.Key.ToLower() == "date_from") && qs.Any(i => i.Key.ToLower() == "date_to"))
			{
				// Get the job messages
				jobMessages = await JobMessageService.GetMostRecent(capcodes, messageType, count, skip);
			}
			else
			{

				// Set the date time values
				DateTime? dateFrom = null;
				DateTime? dateTo = null;

				// Get the query string values
				if (qs.Count(i => i.Key.ToLower() == "date_from") > 0 && !String.IsNullOrEmpty(qs.FirstOrDefault(i => i.Key.ToLower() == "date_from").Value))
				{
					dateFrom = DateTime.ParseExact(qs.FirstOrDefault(x => x.Key == "date_from").Value, "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
				}

				if (qs.Count(i => i.Key.ToLower() == "date_to") > 0 && !String.IsNullOrEmpty(qs.FirstOrDefault(i => i.Key.ToLower() == "date_to").Value))
				{
					dateTo = DateTime.ParseExact(qs.FirstOrDefault(x => x.Key == "date_to").Value, "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
				}

				// Load the job messages between dates
				jobMessages = await JobMessageService.GetMessagesBetweenDates(capcodes, messageType, count, skip, dateFrom, dateTo);

			}

			// Get all the user ids for the progress in te jobs
			List<Guid> allUserIds = new List<Guid>();
			allUserIds.AddRange(jobMessages.SelectMany(i => i.ProgressUpdates.Select(j => j.UserId)).ToList());
			allUserIds = allUserIds.Distinct().ToList();

			// Get all the users that match the ids
			IList<IdentityUser> users = await UserService.GetUsersByIds(allUserIds);

			// return the mapped view models
			IList<JobMessageListItemViewModel> models = new List<JobMessageListItemViewModel>();
			foreach(JobMessage message in jobMessages)
			{

				// Get the capcode
				Capcode capcode = capcodes.FirstOrDefault(i => i.CapcodeAddress == message.Capcode);

				// Add the mapped job message view model
				models.Add(JobMessageListItemViewModel.FromJobMessage(message, capcode, users));
			}

			// return the mapped models
			return models;

		}

		[Route("pager-messages")]
		[HttpGet]
		public async Task<IList<JobMessage>> PagerMessages()
		{
			int count = 50;
			int skip = 0;

			// Get the skip query string
			var queryString = Request.GetQueryNameValuePairs().Where(i => i.Key.ToLower() == "skip");
			if (queryString.Any())
			{
				Int32.TryParse(queryString.First().Value, out skip);
			}
			
			// return the list of messages
			return await JobMessageService.GetMostRecent(count, skip);
		}

		[Route("latest-pager-messages/{lastId}")]
		[HttpGet]
		public async Task<IList<JobMessage>> LatestPagerMessages(Guid lastId)
		{
			return await JobMessageService.GetMostRecent(lastId);
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

		[Route("{id:guid}/notes")]
		[HttpGet]
		public async Task<IList<JobNoteViewModel>> GetNotes(Guid id)
		{

			try
			{

				// Create the job note and return it
				IList<JobNote> notes = await JobMessageService.GetNotesForJob(id);

				// Map the job notes to the list of job notes view models
				IList<JobNoteViewModel> jobNotesModels = await JobMessageModelHelper.MapJobNotesToViewModel(notes, UserService);
				
				return jobNotesModels;

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Error getting notes for the job message. Message: {0}", ex.Message), ex);
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
			}

		}

		[Route("{id:guid}/progress")]
		[HttpPost]
		public async Task<MessageProgressResponseModel> PostProgress(Guid id, PostProgressViewModel model)
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

				// Get the job based on the id
				JobMessage job = await JobMessageService.GetById(id);

				// Ensure the version matches, otherwise return version mismatch error
				if (job.Version != model.Version)
				{
					return new MessageProgressResponseModel()
					{
						Success = false,
						ErrorMessage = "Someone else has changed this job since you loaded it. Reload the job to view changes."
					};
				}

				// Get the current date time
				DateTime progressDateTime = DateTime.Now;

				// Get the date from the posted string value if it exists
				if (!String.IsNullOrEmpty(model.ProgressDateTime))
				{
					progressDateTime = DateTime.ParseExact(model.ProgressDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).ToUniversalTime();
				}

				// Create the progress object and return it
				MessageProgress progress = await JobMessageService.SaveProgress(id, progressDateTime, UserId, model.ProgressType);
				return new MessageProgressResponseModel()
				{
					Timestamp = progress.Timestamp,
					UserId = UserId,
					UserFullName = user.FullName,
					Success = true,
					NewVersion = model.Version + 1
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

		[Route("{id:guid}/progress/delete")]
		[HttpPost]
		public async Task<MessageProgressResponseModel> DeleteProgressType(Guid id, PostProgressViewModel model)
		{

			try
			{

				// Get the job based on the id
				JobMessage job = await JobMessageService.GetById(id);

				// Ensure the version matches, otherwise return version mismatch error
				if (job.Version != model.Version)
				{
					return new MessageProgressResponseModel()
					{
						Success = false,
						ErrorMessage = "Someone else has changed this job since you loaded it. Reload the job to view changes."
					};
				}

				// Clear the progress type for the job
				await JobMessageService.RemoveProgress(id, model.ProgressType);

				// return a success value.
				return new MessageProgressResponseModel()
				{
					Success = true,
					NewVersion = model.Version + 1
				};

			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Cannot remove progress from cancelled job.", ex.Message), ex);
				return new MessageProgressResponseModel()
				{
					Success = false,
					ErrorMessage = "Error removing progress from the job message."
				};
			}

		}

		[Route("latest-from-last-id")]
		[HttpPost]
		public async Task<IList<JobMessageListItemViewModel>> GetLatestMessagesFromLast(PostGetLatestFromLastModel model)
		{

			try
			{

				// Get the capcodes for the current user
				IList<Capcode> capcodes = await CapcodeService.GetCapcodesForUser(UserId);
				IList<string> capcodeStrings = capcodes.Select(i => i.CapcodeAddress).ToList();

				// Get the latest messages
				IList<JobMessage> latestMessages = await JobMessageService.GetLatestFromLastMessage(model.LastMessageId, capcodeStrings, model.MessageType);

				// Create the list of job message view models
				IList<JobMessageListItemViewModel> latestMessagesModels = new List<JobMessageListItemViewModel>();

				// Map the job messages to the JobMesageViewModel type
				if (latestMessages != null && latestMessages.Count > 0)
				{
					
					// Iterate through each message and add to the list
					foreach(JobMessage message in latestMessages)
					{
						// Get the capcode unit name from the list of capcodes
						Capcode capcode = capcodes.FirstOrDefault(i => i.CapcodeAddress == message.Capcode);

						// Map to the JobMessageViewModel
						latestMessagesModels.Add(JobMessageListItemViewModel.FromJobMessage(message, capcode, null));
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

		[Route("{id:guid}/distance-from-job/{otherJobNumber}")]
		[HttpGet]
		public async Task<GetDistanceFromJobModel> GetDistanceFromOtherJob(Guid id, string otherJobNumber)
		{
			// Get the current job
			JobMessage currentJob = await JobMessageService.GetById(id);

			// If the job is null, or there is no location data, then return error result
			if (currentJob == null)
			{
				return new GetDistanceFromJobModel()
				{
					Success = false,
					Error = "Current job cannot be found."
				};
			}
			else if (currentJob.Location == null || currentJob.Location.Coordinates == null)
			{
				return new GetDistanceFromJobModel()
				{
					Success = false,
					Error = "Current job does not contain any location information."
				};
			}

			// Get the other job
			JobMessage otherJob = await JobMessageService.GetByJobNumber(otherJobNumber);

			// If the job is null, or there is no location data, then return error result
			if (otherJob == null)
			{
				return new GetDistanceFromJobModel()
				{
					Success = false,
					Error = "Requested job cannot be found."
				};
			}
			else if (otherJob == null || otherJob.Location == null || otherJob.Location.Coordinates == null)
			{
				return new GetDistanceFromJobModel()
				{
					Success = false,
					Error = "Requested job does not contain any location information."
				};
			}

			// If they are the same job numbers just return 0 distance
			if (otherJob.JobNumber.Equals(currentJob.JobNumber, StringComparison.CurrentCultureIgnoreCase))
			{
				return new GetDistanceFromJobModel()
				{
					Success = true,
					Distance = 0,
					ReferencedJobId = otherJob.Id,
					ReferencedJobNumber = otherJob.JobNumber
				};
			}

			// return the list of coordinates from the google geocode result
			GoogleDirectionsWrapper directionsWrapper = new GoogleDirectionsWrapper(Log);
			DirectionsInfo directions = await directionsWrapper.GetDirectionsCoordinates(currentJob.Location.Coordinates, otherJob.Location.Coordinates);

			// If there is no directions returned, then the directions couldn't be found
			if (directions == null)
			{
				return new GetDistanceFromJobModel()
				{
					Success = false,
					Error = "Distance information could not be found between the two jobs."
				};
			}
			else
			{
				return new GetDistanceFromJobModel()
				{
					Success = true,
					Distance = directions.TotalDistance,
					ReferencedJobId = otherJob.Id,
					ReferencedJobNumber = otherJob.JobNumber
				};
			}
		}

	}
}
