using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Spatial
{

	public class Coordinates
	{

		/// <summary>
		/// The radius of the earth, in KM.
		/// </summary>
		private const double EarthRadiusKms = 6371.01;

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public Coordinates()
		{

		}

		public Coordinates(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		public override string ToString()
		{
			return String.Format("{0}, {1}", Latitude, Longitude);
		}

		/// <summary>
		/// Gets the coordinates from the start point at 'distance' (km) and bearing (degrees).
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="bearing"></param>
		/// <returns></returns>
		public Coordinates PointAtDistanceFrom(double distance, double bearing)
		{

			// Get the distance ratio relative to eath radius, and the sin and cos of that ratio
			double distRatio = distance / EarthRadiusKms;
			double distRatioSine = Math.Sin(distRatio);
			double distRatioCos = Math.Cos(distRatio);

			// Convert the start point into radians.
			double startLatRad = DegreesToRadians(Latitude);
			double startLonRad = DegreesToRadians(Longitude);

			// Get the cos and sin of the start latitude
			double startLatSine = Math.Sin(startLatRad);
			double startLatCos = Math.Cos(startLatRad);

			double bearingRadians = DegreesToRadians(bearing);

			// Get the end lat in radians
			double endLatRads = Math.Asin((startLatSine * distRatioCos) + (startLatCos * distRatioSine * Math.Cos(bearingRadians)));
			double endLonRads = startLonRad + Math.Atan2((Math.Sin(bearingRadians) * distRatioSine * startLatCos), (distRatioCos - startLatSine * Math.Sin(endLatRads)));

			return new Coordinates(RadiansToDegrees(endLatRads), RadiansToDegrees(endLonRads));

		}

		private static double DegreesToRadians(double degrees)
		{
			const double degToRadFactor = Math.PI / 180;
			return degrees * degToRadFactor;
		}

		private static double RadiansToDegrees(double radians)
		{
			const double radToDegFactor = 180 / Math.PI;
			return radians * radToDegFactor;
		}

		public bool IsEmpty()
		{
			return (Latitude == 0 && Longitude == 0);
		}

	}
}
