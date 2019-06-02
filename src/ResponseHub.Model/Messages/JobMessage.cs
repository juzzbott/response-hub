using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.Model.Attachments;

namespace Enivate.ResponseHub.Model.Messages
{
	public class JobMessage : IEntity
	{

		public Guid Id { get; set; }

		public DateTime Timestamp { get; set; }

		public IList<MessageCapcode> Capcodes { get; set; }

		public string MessageContent { get; set; }

		public IList<AdditionalMessage> AdditionalMessages { get; set; }

		public string JobNumber { get; set; }

		public LocationInfo Location { get; set; }

		public IList<JobNote> Notes { get; set; }

		public IList<MessageProgress> ProgressUpdates { get; set; }

		public IList<Guid> AttachmentIds { get; set; }

		public MessageType Type { get; set; }

		public int Version { get; set; }

        public string UniqueHash { get; set; }

		public JobMessage()
		{

			// Instantiate the id
			Id = Guid.NewGuid();

			// Default to version 1
			Version = 1;

			Notes = new List<JobNote>();
			ProgressUpdates = new List<MessageProgress>();

			AttachmentIds = new List<Guid>();

			AdditionalMessages = new List<AdditionalMessage>();

            Capcodes = new List<MessageCapcode>();
		}

	}
}
