using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Spatial;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Messages;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Messages
{
	public class JobMessageDto : IEntity
	{

		public Guid Id { get; set; }

		public DateTime Timestamp { get; set; }

		public string Capcode { get; set; }

		public string MessageContent { get; set; }

		public IList<AdditionalMessage> AdditionalMessages { get; set; }

		public string JobNumber { get; set; }

		public LocationInfoDto Location { get; set; }

		public MessagePriority Priority { get; set; }

		public IList<JobNote> Notes { get; set; }

		public IList<MessageProgress> ProgressUpdates { get; set; }

		public IList<Guid> AttachmentIds { get; set; }

		public MessageType Type { get; set; }

		public int Version { get; set; }

		public JobMessageDto()
		{

			// Instantiate the id
			Id = Guid.NewGuid();

			// Default to version
			Version = 1;

			// Default to administration.
			Priority = MessagePriority.Administration;

			Notes = new List<JobNote>();
			ProgressUpdates = new List<MessageProgress>();

			AttachmentIds = new List<Guid>();

			AdditionalMessages = new List<AdditionalMessage>();
		}

	}
}
