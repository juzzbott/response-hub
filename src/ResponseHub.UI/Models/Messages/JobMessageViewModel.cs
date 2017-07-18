using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.Attachments;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Spatial;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Messages
{
	public class JobMessageViewModel
	{

		public Guid Id { get; set; }

		public string MessageBody { get; set; }

		public string MessageBodyTruncated
		{
			get
			{
				return MessageBody.Truncate(60, "...");
			}
		}

		public IList<AdditionalMessage> AdditionalMessages { get; set; }

		public DateTime Timestamp { get; set; }

		public string JobNumber { get; set; }

		public string Capcode { get; set; }

		public string CapcodeUnitName { get; set; }

		public Guid CapcodeUnitId { get; set; }

		public MessagePriority Priority { get; set; }

		public LocationInfo Location { get; set; }

		public MessageProgressViewModel OnRoute { get; set; }

		public MessageProgressViewModel OnScene { get; set; }

		public MessageProgressViewModel JobClear { get; set; }

		public MessageProgressViewModel Cancelled { get; set; }

		public IList<JobNoteViewModel> Notes { get; set; }

		public IList<Attachment> Attachments { get; set; }

		public IList<Attachment> ImageAttachments { get; set; }

		public IList<JobMessageSignInEntry> SignIns { get; set; }

		[Required(ErrorMessage = "Please ensure you have entered a date.")]
		public string EditProgressDate { get; set; }

		[Required(ErrorMessage = "Please ensure you have entered a time.")]
		public string EditProgressTime { get; set; }

		public Coordinates LhqCoordinates { get; set; }

		public double? DistanceFromLhq { get; set; }

		[Required(ErrorMessage = "You must enter a job number to get the distance from.")]
		public string DistanceFromJobNumber { get; set; }

		public int Version { get; set; }

		public JobMessageViewModel()
		{
			Location = new LocationInfo();
			Notes = new List<JobNoteViewModel>();
			Attachments = new List<Attachment>();
			ImageAttachments = new List<Attachment>();
			SignIns = new List<JobMessageSignInEntry>();
			AdditionalMessages = new List<AdditionalMessage>();

			// Set the default date and time values
			EditProgressDate = DateTime.Now.ToString("yyyy-MM-dd");
			EditProgressTime = DateTime.Now.ToString("HH:mm:ss");
		}
	}
}