using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Attachments
{
	public class Attachment : IEntity
	{

		public Guid Id { get; set; }

		public DateTime Created { get; set; }

		public Guid CreatedBy { get; set; }

		public string Filename { get; set; }

		public string MimeType { get; set; }

		public long FileLength { get; set; }

		/// <summary>
		/// Only populated when the full file data is retrieved. If used within methods that return lists of attachments, this field will be null.
		/// </summary>
		public byte[] FileData { get; set; }

		public Attachment()
		{
			Id = Guid.NewGuid();
		}

	}

}