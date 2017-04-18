using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.WeatherData
{
	public class ObservationData
	{

		public string Name { get; set; }

		public string ProductId { get; set; }

		public DateTime LocalTime { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public string Cloud { get; set; }

		public double Temperature { get; set; }

		public double ApparentTemperature { get; set; }

		public double WindSpeed { get; set; }

		public double WindGustSpeed { get; set; }

		public string WindDirection { get; set; }

		public double Pressure { get; set; }

		public double RainTrace { get; set; }

		public double RelativeHumidity { get; set; }

	}
}
