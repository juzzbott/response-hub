using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.RadarImages.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class RadarImageService : IRadarImageService
	{

		ILogger _log;

		public RadarImageService(ILogger log)
		{
			_log = log;
		}

		/// <summary>
		/// Gets the radar image urls for the specific product id from the BoM public FTP.
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
		public IList<string> GetRadarImagesForProduct(string productId)
		{

			// Create the list to store the image files.
			IList<string> imageFiles = new List<String>();

			try
			{

				string ftpLocation = ConfigurationManager.AppSettings[ConfigurationKeys.BomRadarImageFtpLocation];

				FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(ftpLocation);
				request.Method = WebRequestMethods.Ftp.ListDirectory;
				request.Credentials = new NetworkCredential("anonymous", "anonymous");

				// Get the response
				FtpWebResponse response = (FtpWebResponse)request.GetResponse();


				// Read the details of the response into a string
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{
					// While there is still data to read
					while (reader.Peek() >= 0)
					{
						// Read the filename from the line
						string fileName = reader.ReadLine();

						// If the filename contains the product id, then add to the list, otherwise just continue to next
						if (fileName.ToUpper().Contains(productId.ToUpper() + ".T."))
						{
							imageFiles.Add(fileName);
						}
					}
				}

			}
			catch (Exception ex)
			{
				// Log the exception
				_log.Error("Unable to get FTP file list from BoM FTP. Message: " + ex.Message, ex);
			}

			return imageFiles;
		}
	}
}
