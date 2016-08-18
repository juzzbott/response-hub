using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Attachments;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Attachments;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface IAttachmentRepository : IRepository<AttachmentDto>
	{

		Task StoreAttachment(Attachment attachment);

		Task<Attachment> GetFullAttachment(Guid id);

		Task<IList<Attachment>> GetAttachmentsById(IEnumerable<Guid> ids);

	}
}
