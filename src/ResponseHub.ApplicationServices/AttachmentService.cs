﻿using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.Drawing.Drawing2D;
using Enivate.ResponseHub.Common.Constants;
using System.Web;

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

		public async Task<Attachment> GetAttachmentById(Guid id, bool includeFileData)
		{
			return await _attachmentRepository.GetAttachmentById(id, includeFileData);
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
				Attachment fullAttachment = await _attachmentRepository.GetAttachmentById(attachment.Id, true);
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
		
		/// <summary>
		/// Generates the attachment thumbnail, respecting aspect ratio.
		/// </summary>
		/// <param name="attachment"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public async Task<byte[]> GetThumbnail(Attachment attachment, int width, int height, bool cropImage)
		{

			// Generate the thumbnail filename
			string ext = Path.GetExtension(attachment.Filename);
			string idString = attachment.Id.ToString();
			string cachePath = GetConfigurationPath(ConfigurationManager.AppSettings["Attachment.ThumbnailDirectory"]);
			string thumbnailPath = String.Format("{0}\\{1}\\{2}\\{3}", cachePath, idString[0], idString[1], idString[2]);
			string thumbnailFilename = String.Format("{0}_thumb{1}{2}", attachment.Id, (cropImage ? "_crop" : ""), ext);
			string thumbnailFullPath = String.Format("{0}\\{1}", thumbnailPath, thumbnailFilename);

			// Validate the extension before generating thumbnail
			if (!GeneralConstants.ImageExtensions.Contains(ext.ToLower()))
			{
				throw new ApplicationException("Cannot generate thumbnail for non-image file type.");
			}

			// Check if the thumbnail exists on the filesystem.
			if (File.Exists(thumbnailFullPath))
			{
				return File.ReadAllBytes(thumbnailFullPath);
			}

			Attachment fullAttachment = await GetAttachmentById(attachment.Id, true);

			// Get the byte array
			byte[] thumbnailBytes = GetThumbnailImageData(fullAttachment.FileData, width, height, cropImage);
			
			// Ensure thumbnail path exists
			if (!Directory.Exists(thumbnailPath))
			{
				Directory.CreateDirectory(thumbnailPath);
			}

			// Save the thumbnail to disk
			File.WriteAllBytes(thumbnailFullPath, thumbnailBytes);

			// return the thumbnail bytes
			return thumbnailBytes;

		}

		private byte[] GetThumbnailImageData(byte[] fileData, int width, int height, bool cropImage)
		{
			using (MemoryStream msInput = new MemoryStream(fileData))
			{
				float ratio;
				SizeF newSize = new SizeF();
				Rectangle cropRectangle = new Rectangle();

				Bitmap srcBmp = new Bitmap(msInput);

				if (srcBmp.Width >= srcBmp.Height)
				{

					
					if (cropImage)
					{
						// Get ratio for the new thumbnail
						ratio = ((float)srcBmp.Width / (float)srcBmp.Height);
						float cropWidth = srcBmp.Width;
						float cropHeight = (srcBmp.Height / ratio);
						float cropYStart = ((srcBmp.Height - cropHeight) / 2);
						int cropXStart = 0;

						// new size and cropSource for landscape, crop and resize
						newSize = new SizeF(width, height);
						cropRectangle = new Rectangle(cropXStart, (int)cropYStart, (int)cropWidth, (int)cropHeight);
					}
					else
					{
						// Get ratio for landscape
						ratio = ((float)srcBmp.Height / (float)srcBmp.Width);
						// new size for landscape, resize only
						newSize = new SizeF(width, width * ratio);
					}
				}
				else
				{

					if (cropImage)
					{
						// Get ratio for the new thumbnail
						ratio = ((float)srcBmp.Height / (float)srcBmp.Width);
						float cropWidth = srcBmp.Width;
						float cropHeight = (srcBmp.Height / ratio);
						float cropYStart = ((srcBmp.Height - cropHeight) / 2);
						int cropXStart = 0;

						// new size and cropSource for portrait, crop and resize
						newSize = new SizeF(width, height);
						cropRectangle = new Rectangle((int)cropXStart, (int)cropYStart, (int)cropWidth, (int)cropHeight);
					}
					else
					{
						// Get ration of portrait
						ratio = ((float)srcBmp.Width / (float)srcBmp.Height);
						// new size for portrait, resize only
						newSize = new SizeF(height * ratio, height);
					}
				}
				Bitmap target = new Bitmap((int)newSize.Width, (int)newSize.Height);

				using (Graphics graphics = Graphics.FromImage(target))
				{
					graphics.Clear(Color.White);
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					graphics.CompositingQuality = CompositingQuality.HighQuality;
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.CompositingMode = CompositingMode.SourceOver;
					graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
					

					ImageAttributes ia = new ImageAttributes();
					ia.SetWrapMode(WrapMode.TileFlipXY);

					if (cropImage)
					{
						// Crop and resize
						graphics.DrawImage(srcBmp, new Rectangle(0, 0, (int)newSize.Width, (int)newSize.Height), cropRectangle, GraphicsUnit.Pixel);
					}
					else
					{
						// Just resize
						graphics.DrawImage(srcBmp, 0, 0, newSize.Width, newSize.Height);
					}


					using (MemoryStream msOutput = new MemoryStream())
					{
						target.Save(msOutput, ImageFormat.Jpeg);
						return msOutput.ToArray();
					}
				}

			}
		}

		private string GetConfigurationPath(string path)
		{

			// If the directory starts with ~, its a virtual path, so use the Server.MapPath to get full directory
			if (path[0] == '~' && HttpContext.Current != null)
			{
				path = HttpContext.Current.Server.MapPath(path);
			}
			else if (path[0] == '.')
			{
				// Log directory is a relative path (../ or ./) so append to current directory
				path = String.Format("{0}\\{1}", Environment.CurrentDirectory, path.Substring(1));
			}

			// return the path
			return path;

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

					// Set the image property
					image.SetPropertyItem(orientationProp);
				}


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
