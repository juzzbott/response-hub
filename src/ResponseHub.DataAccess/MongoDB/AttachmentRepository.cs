using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson.IO;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Attachments;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Attachments;
using MongoDB.Driver.GridFS;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{
	[MongoCollectionName(GridFSBucketName)]
	public class AttachmentRepository : MongoRepository<AttachmentDto>, IAttachmentRepository
	{

		private const string GridFSBucketName = "attachments";

		/// <summary>
		/// Gets the full attachment, including file data from the database.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<Attachment> GetFullAttachment(Guid id)
		{
			// Find the attachment dto object based on the id
			AttachmentDto attachment = await FindOne(i => i.Id == id);

			// If the attachment is null, return null
			if (attachment == null)
			{
				return null;
			}

			// Create the attachment model
			Attachment model = MapDtoToModel(attachment);

			// Get the grid file based on the grid file id
			IGridFSBucket bucket = new GridFSBucket(Collection.Database, new GridFSBucketOptions { BucketName = GridFSBucketName });
			model.FileData = await bucket.DownloadAsBytesAsync(attachment.GridFSId);

			// return the attachment model
			return model;

		}

		/// <summary>
		/// Gets the collection of attachments as a list.
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public async Task<IList<Attachment>> GetAttachmentsById(IEnumerable<Guid> ids)
		{

			// Build the query
			FilterDefinition<AttachmentDto> filter = Builders<AttachmentDto>.Filter.In(i => i.Id, ids);

			// Create the sort order
			SortDefinition<AttachmentDto> sort = Builders<AttachmentDto>.Sort.Descending(i => i.Created);
			
			// Get the results from the database
			IList<AttachmentDto> results = await Collection.Find(filter).Sort(sort).ToListAsync();

			// return the mapped results
			return results.Select(i => MapDtoToModel(i)).ToList();
		}

		/// <summary>
		/// Stores the attachment, including file data, into the database.
		/// </summary>
		/// <param name="attachment"></param>
		/// <returns></returns>
		public async Task StoreAttachment(Attachment attachment)
		{
			// Write the binary data to the GridFS store
			IGridFSBucket bucket = new GridFSBucket(Collection.Database, new GridFSBucketOptions { BucketName = GridFSBucketName });
			ObjectId fileId = await bucket.UploadFromBytesAsync(attachment.Filename, attachment.FileData);

			// Write the attachement to the Attachments collection
			await Save(MapModelToDto(attachment, fileId));

		}

		/// <summary>
		/// Maps the AttachmentDto data object object to the Attachment model object
		/// </summary>
		/// <param name="dataObject"></param>
		/// <returns></returns>
		private Attachment MapDtoToModel(AttachmentDto dataObject)
		{
			return new Attachment {
				Created = dataObject.Created,
				CreatedBy = dataObject.CreatedBy,
				FileLength = dataObject.FileLength,
				Filename = dataObject.Filename,
				Id = dataObject.Id,
				MimeType = dataObject.MimeType
			};
		}

		/// <summary>
		/// Maps the Attachment model to the AttachmentDto data object.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private AttachmentDto MapModelToDto(Attachment model, ObjectId gridFsId)
		{
			return new AttachmentDto
			{
				Created = model.Created,
				CreatedBy = model.CreatedBy,
				FileLength = model.FileLength,
				Filename = model.Filename,
				GridFSId = gridFsId,
				Id = model.Id,
				MimeType = model.MimeType
			};
		}

	}
}
