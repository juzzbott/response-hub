using Enivate.ResponseHub.Model.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Attachments.Interface
{
	public interface IAttachmentService
	{

		Task<Attachment> SaveAttachment(string filename, Guid userId, Stream fileData, string mimeType);

		Task<IList<Attachment>> GetAttachmentsById(IEnumerable<Guid> ids);

		Task<Attachment> GetAttachmentById(Guid id, bool includeFileData);

		Task<byte[]> GetAllJobAttachments(JobMessage job);

		Task<byte[]> GetResizedImage(Attachment attachment, int width, int height, bool croppedcropImage);

		void ClearAttachmentCache();

		void ClearAttachmentCache(Guid attachmentId);

	}
}
