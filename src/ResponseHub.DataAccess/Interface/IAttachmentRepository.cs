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

		Task<Attachment> GetAttachmentById(Guid id, bool includeFileData);

		Task<IList<Attachment>> GetAttachmentsById(IEnumerable<Guid> ids);

        Task<IList<Attachment>> GetAttachmentsByUserId(Guid userId);


    }
}
