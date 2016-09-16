using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.Attachments;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Spatial;
using System;
using System.Collections.Generic;
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

		public DateTime Timestamp { get; set; }

		public string JobNumber { get; set; }

		public string Capcode { get; set; }

		public string CapcodeGroupName { get; set; }

		public Guid CapcodeGroupId { get; set; }

		public MessagePriority Priority { get; set; }

		public LocationInfo Location { get; set; }

		public MessageProgressViewModel OnRoute { get; set; }

		public MessageProgressViewModel OnScene { get; set; }

		public MessageProgressViewModel JobClear { get; set; }

		public MessageProgressViewModel Cancelled { get; set; }

		public IList<JobNoteViewModel> Notes { get; set; }

		public IList<Attachment> Attachments { get; set; }

		public IList<Attachment> ImageAttachments { get; set; }

		public JobMessageViewModel()
		{
			Location = new LocationInfo();
			Notes = new List<JobNoteViewModel>();
			Attachments = new List<Attachment>();
			ImageAttachments = new List<Attachment>();
		}
	}
}