using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model;
using MongoDB.Bson;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Attachments
{
	public class AttachmentDto : IEntity
	{
		public Guid Id { get; set; }

		public ObjectId GridFSId { get; set; }

		public DateTime Created { get; set; }

		public Guid CreatedBy { get; set; }

		public string Filename { get; set; }

		public string MimeType { get; set; }

		public long FileLength { get; set; }

	}
}
