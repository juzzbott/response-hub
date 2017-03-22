using Enivate.ResponseHub.Model.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.PagerDecoder.ApplicationServices
{
	public class SpatialUtility
	{

		/// <summary>
		/// Gets the coordinates from the grid reference based at the given scale.
		/// </summary>
		/// <param name="coordinates"></param>
		/// <param name="gridRef"></param>
		/// <param name="scale"></param>
		/// <returns></returns>
		public static Coordinates GetCoordinatesFromGridReference(Coordinates coordinates, string gridRef, int scale)
		{

			// Get the grid part distance, in km, from the scale of the map
			double gridPartDist = GetGridPartDistFromScale(scale);

			// If the gridPartDist is 0, we have an invalid scale, so just return the original coords.
			if (gridPartDist == 0)
			{
				return coordinates;
			}
			// Get the bottom the bottom right coordinate
			// bearing is 225 deg (bottom left corner from 0)
			double distToCorner = GetDistanceToCorner(scale);
			Coordinates bottomRight = coordinates.PointAtDistanceFrom(distToCorner, 225);

			// Northing ~ Latitude ~ Y ; Easting ~ Longitude ~ X

			// Get the grid square part numbers as the 3 and 6 char in the grid ref, which is a 6 figure grid ref
			int eastingPart = Int32.Parse(gridRef.Substring(2, 1));
			int northingPart = Int32.Parse(gridRef.Substring(5, 1));

			// there are 10 parts to each grid ref, and the 3&6th number indicate this part.
			// So, we take the bottom left coords as the base, and get the distances based on the grid parts
			double latDist = northingPart * gridPartDist;
			double lonDist = eastingPart * gridPartDist;
			double actualLat = bottomRight.PointAtDistanceFrom(latDist, 0).Latitude;
			double actualLon = bottomRight.PointAtDistanceFrom(lonDist, 90).Longitude;

			return new Coordinates(actualLat, actualLon);

		}

		/// <summary>
		/// Gets the distance, in km, of the grid parts based on the map scale.
		/// </summary>
		/// <param name="scale"></param>
		/// <returns></returns>
		public static double GetGridPartDistFromScale(int scale)
		{
			switch (scale)
			{

				case 15000:
					return 0.03;

				case 20000:
					return 0.04;

				case 50000:
					return 0.1;

				case 100000:
					return 0.1;

				case 150000:
					return 0.1;
			}

			// No case found, so return 0
			return 0;
		}

		/// <summary>
		/// Gets the distance to the corner from the center of the grid square.
		/// </summary>
		/// <param name="scale">The scale of the map to get the distance to the corner.</param>
		/// <returns></returns>
		public static double GetDistanceToCorner(int scale)
		{
			// For example, at scale 50000, each grid square is 1km, and the original point is in the centre of that square
			// So to get the bottom left corner, we need to 707.11 meters at bearing 225 deg (bottom left corner from 0).
			// 707.11 is the hypotenuse of a right angle triangle with sides 500 meters long.

			// Get the grid part distance
			double gridPartDist = GetGridPartDistFromScale(scale);

			// Get the straight line distance from the centre, which is 5 parts
			double straightLineDist = gridPartDist * 5;

			// Calculate the hypotenuse of a right angle triangle with length of straightLineDist
			double distToCorner = Math.Sqrt(Math.Pow(straightLineDist, 2) + Math.Pow(straightLineDist, 2));

			// return the distance
			return distToCorner;
		}
		/// <summary>
		/// Converts the degree value into a radian value unit.
		/// </summary>
		/// <param name="degrees">The degree value to return as radian</param>
		/// <returns></returns>
		public static double DegreesToRadians(double degree)
		{
			return (degree * Math.PI / 180.0F);
		}

		/// <summary>
		/// Converts the radian value into a degree value unit.
		/// </summary>
		/// <param name="degrees">The radian value to return as degree</param>
		/// <returns></returns>
		public static double RadiansToDegrees(double radian)
		{
			return (radian / Math.PI * 180.0F);
		}

		public static double DistanceBetweenPoints(double lat1, double long1, double lat2, double long2)
		{
			return DistanceBetweenPoints(lat1, long1, lat2, long2, DistanceUnitType.Kilometers);
		}

		public static double DistanceBetweenPoints(double lat1, double long1, double lat2, double long2, DistanceUnitType unitType)
		{

			// Calculate the theta
			double theta = long1 - long2;

			// Calculate the distance in radians
			double dist = Math.Sin(DegreesToRadians(lat1)) * Math.Sin(DegreesToRadians(lat2)) + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) * Math.Cos(DegreesToRadians(theta));
			dist = Math.Acos(dist);

			// Convert distance to degrees
			dist = RadiansToDegrees(dist);

			// Add earth diameter
			dist = dist * 60 * 1.1515;

			switch (unitType)
			{

				case DistanceUnitType.Kilometers:
					return dist * 1.609344;

				case DistanceUnitType.NauticalMiles:
					return dist * 0.8684;

				case DistanceUnitType.Miles:
					return dist;
					
			}

			return dist;
		}

	}
}
