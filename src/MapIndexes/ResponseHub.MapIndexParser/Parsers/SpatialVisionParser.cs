using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DotSpatial.Data;
using DotSpatial.Projections;

using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.MapIndexParser.Parsers
{
	public class SpatialVisionParser : IMapIndexParser
	{

		public IDictionary<string, MapIndex> MapIndexes { get; set; }

		public SpatialVisionParser()
		{
			// Instantiate the map indexes.
			MapIndexes = new Dictionary<string, MapIndex>();
		}

		/// <summary>
		/// Processes a list of shapefiles to be inserted into the mongo 
		/// </summary>
		/// <param name="shapeFileListPath">The file path to the shape file list.</param>
		public void ProcessShapeFileList(string shapeFileListPath)
		{
			// If the shapeFileListPath is null or empty, show error message
			if (String.IsNullOrEmpty(shapeFileListPath))
			{
				Console.WriteLine("Shapefile list path is empty.");
				return;
			}

			// If the path to the shape file list does not exist, display error and return
			if (!File.Exists(shapeFileListPath))
			{
				Console.Write(String.Format("Shapefile list path '{0}' not found.", shapeFileListPath));
				return;
			}

			IDictionary<string, MapType> shapeFiles = new Dictionary<string, MapType>();
			bool validFile = true;
			// Open the file for reading and read all the lines into a list of strings
			using (StreamReader reader = new StreamReader(shapeFileListPath))
			{
				string line;
				// Read the lines until there is no more lines.
				while ((line = reader.ReadLine()) != null)
				{

					string[] lineParts = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

					// If there are not 2 elements in the array, then the list file is invalid.
					if (lineParts.Length != 2)
					{
						validFile = false;
					}
					else
					{

						// If the key already exists, skip it so we are not processing the same file twice
						if (shapeFiles.Keys.Any(i => i.ToLower() == lineParts[1].ToLower()))
						{
							Console.WriteLine(String.Format("Skipping duplicate file '{0}'", lineParts[1]));
							continue;
						}

						// Get the map type from the first item in the array
						int mapTypeInt;
						Int32.TryParse(lineParts[0], out mapTypeInt);

						// Add to the dictionary of shape files.
						shapeFiles.Add(lineParts[1], (MapType)mapTypeInt);
					}

				}
			}

			if (!validFile)
			{
				Console.WriteLine(String.Format("Invalid list file format detected. List files should be in the format of [1|2]:path_to_shape_file. One entry per line."));
				return;
			}

			// If there are no shape files, warn the user and return
			if (shapeFiles.Count == 0)
			{
				Console.WriteLine("No shapefile paths found in shapefile list.");
				return;
			}

			// Output the amount of shape files.
			Console.WriteLine();
			Console.WriteLine(String.Format("{0} shapefile{1} found. Processing...", shapeFiles.Count, (shapeFiles.Count != 1 ? "s" : "")));

			// For each shape file, process it
			foreach (KeyValuePair<string, MapType> shapeFile in shapeFiles)
			{
				string shapeFilePath = Path.GetFullPath(shapeFile.Key);
				ProcessShapeFile(shapeFilePath, shapeFile.Value);
			}

		}

		/// <summary>
		/// Process the individual shape file.
		/// </summary>
		/// <param name="shapeFilePath">The path to the shapefile.</param>
		/// <param name="mapType">The type of map to parse.</param>
		public void ProcessShapeFile(string shapeFilePath, MapType mapType)
		{

			// Add some spacing to the log
			Console.WriteLine();

			// If the shapeFile is null or empty, show error message
			if (String.IsNullOrEmpty(shapeFilePath))
			{
				Console.WriteLine("Shapefile path is empty.");
				return;
			}

			// If the path to the shape file does not exist, display error and return
			if (!File.Exists(shapeFilePath))
			{
				Console.WriteLine(String.Format("Shapefile path '{0}' not found.", shapeFilePath));
				return;
			}

			if (mapType == MapType.Unknown)
			{
				Console.WriteLine(String.Format("Unknown map type for map file '{0}'"));
				return;
			}

			Console.WriteLine(String.Format("Processing shape file '{0}' - MapType: {1}", shapeFilePath, mapType.GetEnumDescription()));

			// Load the shape file and project to GDA94
			Shapefile indexMapFile = Shapefile.OpenFile(shapeFilePath);
			indexMapFile.Reproject(KnownCoordinateSystems.Geographic.Australia.GeocentricDatumofAustralia1994);
			
			// Set the start date
			DateTime startDate = DateTime.Now;

			int totalPages = 0;
			int totalGridReferences = 0;

			// Get the map index from the Feature data
			for (int i = 0; i < indexMapFile.DataTable.Rows.Count; i++)
			{

				// Get the feature
				IFeature feature = indexMapFile.Features.ElementAt(i);

				// Map the feature to the grid reference
				MapFeatureToMapIndex(feature, mapType, ref totalPages, ref totalGridReferences);
			}

			// Set the end date
			DateTime endDate = DateTime.Now;
			TimeSpan duration = (endDate - startDate);

			Console.WriteLine();
			Console.WriteLine("Index map totals.");
			Console.WriteLine(String.Format("Total pages: {0}", totalPages));
			Console.WriteLine(String.Format("Total grid references: {0}", totalGridReferences));
			Console.WriteLine(String.Format("Duration: {0}", duration));

			// Add some spacing to the log
			Console.WriteLine();
			Console.WriteLine();

		}

		/// <summary>
		/// Maps the shapefile Feature object to a MapIndex file.
		/// </summary>
		/// <param name="feature"></param>
		/// <param name="mapType"></param>
		/// <param name="mapIndexes"></param>
		private void MapFeatureToMapIndex(IFeature feature, MapType mapType, ref int totalPages, ref int totalGridReferences)
		{

			// If the feature or mapIndexes is null, then return 
			if (feature == null)
			{
				return;
			}

			try
			{

				// Get the map values
				string pageNumber = feature.DataRow[2].ToString();

				int mgaZone;
				Int32.TryParse(feature.DataRow[4].ToString(), out mgaZone);

				int scale;
				Int32.TryParse(feature.DataRow[5].ToString(), out scale);

				// Check if the map reference at page number exists... if it doesnt, create it
				if (!MapIndexes.ContainsKey(pageNumber))
				{
					MapIndexes[pageNumber] = new MapIndex()
					{
						MapType = mapType,
						PageNumber = pageNumber,
						Scale = scale,
						UtmNumber = mgaZone
					};
					totalPages++;
				}

				// Create the grid reference
				MapGridReferenceInfo gridRefObj = MapGridReference(feature);

				// Set the grid reference
				MapIndexes[pageNumber].GridReferences.Add(gridRefObj);
				totalGridReferences++;

				Console.WriteLine(String.Format("Parsed map page: {0} Grid reference: {1}", pageNumber, gridRefObj.GridSquare));

			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to parse feature.");
				Console.WriteLine(ex.ToString());
			}

		}

		/// <summary>
		/// Maps the map reference from the GridSquare feature.
		/// </summary>
		/// <param name="feature">The feature to map to the grid reference.</param>
		/// <returns>The mapped grid reference</returns>
		private MapGridReferenceInfo MapGridReference(IFeature feature)
		{
			// Get the grid reference
			string gridReference = feature.DataRow[3].ToString();

			// Get the centre point of the polygon for the grid reference
			IFeature centroid = feature.Centroid();

			// Create the GridReference
			MapGridReferenceInfo gridReferenceObj = new MapGridReferenceInfo()
			{
				GridSquare = gridReference,
				Latitude = centroid.Coordinates[0].Y,
				Longitude = centroid.Coordinates[0].X

			};

			return gridReferenceObj;

		}

	}
}
