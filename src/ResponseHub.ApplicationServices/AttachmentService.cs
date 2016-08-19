using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Attachments;
using Enivate.ResponseHub.Model.Attachments.Interface;
using Enivate.ResponseHub.Model.Messages;

using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Drawing;
using System.Drawing.Imaging;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class AttachmentService : IAttachmentService
	{

		private readonly IAttachmentRepository _attachmentRepository;
		private readonly IJobMessageRepository _jobMessageRepository;

		public AttachmentService(IAttachmentRepository attachmentRepository, IJobMessageRepository jobMessageRepository)
		{
			_attachmentRepository = attachmentRepository;
			_jobMessageRepository = jobMessageRepository;
		}
		
		/// <summary>
		/// Creates the attachment object and stored it into the database. The attachment is then referenced to the job message it was uploaded against.
		/// </summary>
		/// <param name="jobMessageId"></param>
		/// <param name="filename"></param>
		/// <param name="userId"></param>
		/// <param name="fileData"></param>
		/// <param name="mimeType"></param>
		/// <returns></returns>
		public async Task<Attachment> SaveAttachment(string filename, Guid userId, Stream fileData, string mimeType)
		{

			byte[] fileDataBytes;

			// Read the file data from the memory stream
			using (BinaryReader reader = new BinaryReader(fileData))
			{
				fileDataBytes = reader.ReadBytes((int)fileData.Length);
			}

			// Get the extension
			string extension = Path.GetExtension(filename).ToLower();

			// If it's a JPEG, we are going to ensure orientation is correct
			if (extension == ".jpg" || extension == ".jpeg")
			{
				// Ensure correct orientation
				fileDataBytes = EnsureCorrectJpegOrientation(fileDataBytes);
			}

			// Create the Attachment object
			Attachment attachment = new Attachment
			{
				Created = DateTime.UtcNow,
				CreatedBy = userId,
				Filename = filename,
				MimeType = mimeType,
				FileData = fileDataBytes,
				FileLength = (long)fileDataBytes.Length
			};

			// Store the attachment into the database
			await _attachmentRepository.StoreAttachment(attachment);

			// return the attachment
			return attachment;

		}

		public async Task<IList<Attachment>> GetAttachmentsById(IEnumerable<Guid> ids)
		{
			return await _attachmentRepository.GetAttachmentsById(ids);
		}

		public async Task<Attachment> GetAttachmentById(Guid id)
		{
			return await _attachmentRepository.GetFullAttachment(id);
		}

		public async Task<byte[]> GetAllJobAttachments(JobMessage job)
		{
			// Get all the attachments for the job
			IList<Attachment> attachments = await _attachmentRepository.GetAttachmentsById(job.AttachmentIds);

			// Now we need to create the zip archive
			MemoryStream outputMemStream = new MemoryStream();
			ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

			// Set to medium compression
			zipStream.SetLevel(5);

			foreach(Attachment attachment in attachments)
			{

				// Get the full attachment
				Attachment fullAttachment = await _attachmentRepository.GetFullAttachment(attachment.Id);
				using (MemoryStream ms = new MemoryStream(fullAttachment.FileData))
				{
					
					// Create the zip entry
					ZipEntry entry = new ZipEntry(attachment.Filename);
					entry.DateTime = DateTime.Now;

					// Add to the zip file
					zipStream.PutNextEntry(entry);
					StreamUtils.Copy(ms, zipStream, new byte[4096]);
					zipStream.CloseEntry();

				}
				
			}

			// Close the stream 
			zipStream.IsStreamOwner = false;
			zipStream.Close();

			outputMemStream.Position = 0;
			return outputMemStream.ToArray();

		}

		#region Image helpers
		
		private byte[] EnsureCorrectJpegOrientation(byte[] fileDataBytes)
		{

			using (MemoryStream ms = new MemoryStream(fileDataBytes))
			{

				// Create the image from the bytes
				Image image = Image.FromStream(ms);

				// Get the orientation property
				PropertyItem orientationProp = image.PropertyItems.FirstOrDefault(i => i.Id == 0x112);

				// If we need to rotate, then do so
				if (orientationProp != null && orientationProp.Value[0] != 1)
				{

					if (orientationProp.Value[0] == 2)
					{
						image.RotateFlip(RotateFlipType.RotateNoneFlipX);
					}
					else if (orientationProp.Value[0] == 3)
					{
						image.RotateFlip(RotateFlipType.Rotate180FlipNone);
					}
					else if (orientationProp.Value[0] == 4)
					{
						image.RotateFlip(RotateFlipType.Rotate180FlipX);
					}
					else if (orientationProp.Value[0] == 5)
					{
						image.RotateFlip(RotateFlipType.Rotate90FlipX);
					}
					else if (orientationProp.Value[0] == 6)
					{
						image.RotateFlip(RotateFlipType.Rotate90FlipNone);
					}
					else if (orientationProp.Value[0] == 7)
					{
						image.RotateFlip(RotateFlipType.Rotate270FlipX);
					}
					else if (orientationProp.Value[0] == 8)
					{
						image.RotateFlip(RotateFlipType.Rotate270FlipNone);
					}

					// Reset the orientation flag value
					orientationProp.Value = new byte[] { 1, 0 };
				}

				image.SetPropertyItem(orientationProp);

				using (MemoryStream msOutput = new MemoryStream())
				{
					image.Save(msOutput, ImageFormat.Jpeg);
					return msOutput.ToArray();
				}

			}
		}

		#endregion
	}
}
