using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.WeatherData.Interface
{
	public interface IWeatherDataService
	{

		IList<string> GetRadarImagesForProduct(string productId);

		byte[] GetRadarImageBytes(string radarImageFilename);

	}
}
