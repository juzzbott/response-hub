using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.WeatherData.Interface
{
	public interface IWeatherDataService
	{

		IList<string> GetRadarImagesForProduct(string productId, string locationCode);

		void DownloadRadarImageFileListFromFtp(string productId, string locationCode);

		byte[] GetRadarImageBytes(string radarImageFilename);

		void DownloadImageFileFromFtp(string imageFilename, string locationCode);

		IList<ObservationData> GetObservationData(string observationId, string locationCode);

		void DownloadObservationData(string observationId, string locationCode);
		string GetCacheDirectory(string locationCode);

	}
}
