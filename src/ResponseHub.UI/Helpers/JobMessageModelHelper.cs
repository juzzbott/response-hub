using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.UI.Models.Messages;
using Enivate.ResponseHub.Model.Identity.Interface;
using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.UI.Helpers
{
	public class JobMessageModelHelper
	{

		public static async Task<IList<JobNoteViewModel>> MapJobNotesToViewModel(IList<JobNote> jobNotes, IUserService userService)
		{
			
			// Create the list of job note view models
			IList<JobNoteViewModel> jobNotesModels = new List<JobNoteViewModel>();

			// Get the list of user ids from the notes
			if (jobNotes != null && jobNotes.Count > 0)
			{

				// select the distinct user ids from the list
				IList<Guid> userIDs = jobNotes.Select(i => i.UserId).Distinct().ToList();

				// Get the list of users from the ids
				IList<IdentityUser> users = await userService.GetUsersByIds(userIDs);

				// LOop through the notes and map to the view model
				foreach (JobNote note in jobNotes)
				{

					// Create the view model
					JobNoteViewModel noteModel = new JobNoteViewModel
					{
						Body = note.Body,
						Created = note.Created,
						Id = note.Id,
						IsWordBack = note.IsWordBack,
						UserId = note.UserId
					};

					// Get the user from the id
					IdentityUser user = users.FirstOrDefault(i => i.Id == note.UserId);

					// Set the note user name
					if (user != null)
					{
						noteModel.UserDisplayName = user.FullName;
					}

					// Add to list
					jobNotesModels.Add(noteModel);
				}
			}

			// return the list of job notes
			return jobNotesModels;

		}

	}
}