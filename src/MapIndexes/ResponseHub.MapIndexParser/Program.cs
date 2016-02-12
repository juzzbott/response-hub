using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DotSpatial.Data;
using DotSpatial.Projections;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Spatial;
using System.Data;
using MongoDB.Driver;
using Enivate.ResponseHub.DataAccess.MongoDB;
using Enivate.ResponseHub.Logging;
using System.Threading;

namespace Enivate.ResponseHub.MapIndexParser
{
	class Program
	{

		private static string _dbConnectionString;

		private static bool _clearMapIndexes;
		
		static void Main(string[] args)
		{

			// if no args, show help and return
			if (args == null || args.Length == 0)
			{
				// Display the invalid usage message
				DisplayInvalidUsage();
			}
			else if (args[0].ToLower() == "-h")
			{
				// Display the help message
				DisplayHelp();
				return;
			}
			else if (args[0].ToLower() == "-shp")
			{

				// Ensure -db option exists
				if (!EnsureDbConnectionString(args))
				{
					DisplayInvalidUsage();
					return;
				}

				// Check if we need to clear the indexes first.
				_clearMapIndexes = args.Contains("-R");

				MapType mapType = GetMapTypeFromArgument(args);

				// If the map type is unknown, then show the usage information
				if (mapType == MapType.Unknown)
				{
					DisplayInvalidUsage();
					return;
				}

				// Process only a single shapefile
				string shapeFilePath = Path.GetFullPath(args[1]);
				ProcessShapeFile(shapeFilePath, mapType);
			}
			else if (args[0].ToLower() == "-lf")
			{
				
				// Ensure -db option exists
				if (!EnsureDbConnectionString(args))
				{
					DisplayInvalidUsage();
					return;
				}

				// Check if we need to clear the indexes first.
				_clearMapIndexes = args.Contains("-R");

				// Process only a single shapefile
				string shapeFileListPath = Path.GetFullPath(args[1]);

				// Process the list of shape files.
				ProcessShapeFileList(shapeFileListPath);
			}
			else
			{
				// Display invalid usage
				DisplayInvalidUsage();
			}


		}

		/// <summary>
		/// Gets the map type from the argument.
		/// </summary>
		/// <returns></returns>
		private static MapType GetMapTypeFromArgument(string[] args)
		{

			// Get the -db index
			for (int i = 0; i < args.Length; i++)
			{
				// If the args is -map, get the next args, which should be the map type. 
				// if the map type doesn't begin with a '-', then it's the connection string, otherwise it's another argument, and thus invalid.
				if (args[i].ToLower() == "-map")
				{

					// If there is no next args, then return invalid usage.
					if (i + 1 >= args.Length)
					{
						return MapType.Unknown;
					}

					string mapType = args[i + 1];

					// Ensure the db value is valid.
					if (!String.IsNullOrEmpty(mapType) && !mapType.StartsWith("-"))
					{
						int mapTypeInt;
						Int32.TryParse(mapType, out mapTypeInt);
						return (MapType)mapTypeInt;
					}

				}
			}

			return MapType.Unknown;
		}

		/// <summary>
		/// Ensures the -db parameter and value exists.
		/// </summary>
		/// <returns>True if the -db parameter and value exist, otherwise false.</returns>
		private static bool EnsureDbConnectionString(string[] args)
		{

			bool dbParameterExists = false;

			// Get the -db index
			for(int i = 0; i < args.Length; i++)
			{
				// If the args is -db, get the next args, which should be the db connection string. 
				// if the db connection string doesn't begin with a '-', then it's the connection string, otherwise it's another argument, and thus invalid.
				if (args[i].ToLower() == "-db")
				{

					// If there is no next args, then return invalid usage.
					if (i + 1 >= args.Length)
					{
						return false;
					}

					string dbConn = args[i + 1];

					// Ensure the db value is valid.
					if (!String.IsNullOrEmpty(dbConn) && !dbConn.StartsWith("-"))
					{
						_dbConnectionString = dbConn;
						return true;
					}

				}
			}

			return dbParameterExists;
		}

		#region Shape file parsing

		/// <summary>
		/// Processes a list of shapefiles to be inserted into the mongo 
		/// </summary>
		/// <param name="shapeFileListPath">The file path to the shape file list.</param>
		private static void ProcessShapeFileList(string shapeFileListPath)
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
			foreach(KeyValuePair<string, MapType> shapeFile in shapeFiles)
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
		private static void ProcessShapeFile(string shapeFilePath, MapType mapType)
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

			Console.WriteLine(String.Format("Processing shape file '{0}' - MapType: {1}", shapeFilePath, EnumValue.GetEnumDescription(mapType)));

			// Load the shape file and project to GDA94
			Shapefile indexMapFile = Shapefile.OpenFile(shapeFilePath);
			indexMapFile.Reproject(KnownCoordinateSystems.Geographic.Australia.GeocentricDatumofAustralia1994);

			// Create the dictionary of map indexes
			IDictionary<string, MapIndex> mapIndexes = new Dictionary<string, MapIndex>();

			// Set the start date
			DateTime startDate = DateTime.Now;

			// Get the map index from the Feature data
			for(int i = 0; i < indexMapFile.DataTable.Rows.Count; i++)
			{

				// Get the feature
				IFeature feature = indexMapFile.Features.ElementAt(i);

				// Map the feature to the grid reference
				MapFeatureToMapIndex(feature, mapType, ref mapIndexes);
			}

			// Set the end date
			DateTime endDate = DateTime.Now;

			int totalPages = mapIndexes.Count;
			int totalGridReferences = mapIndexes.Sum(i => i.Value.GridReferences.Count);
			double gridReferenesPerPage = ((double)totalGridReferences / (double)totalPages);
			TimeSpan duration = (endDate - startDate);

			Console.WriteLine();
			Console.WriteLine("Index map totals.");
			Console.WriteLine(String.Format("Total pages: {0}", totalPages));
			Console.WriteLine(String.Format("Total grid references: {0}", totalGridReferences));
			Console.WriteLine(String.Format("Grid references per page: {0}", gridReferenesPerPage));
			Console.WriteLine(String.Format("Duration: {0}", duration));

			InsertMapIndexes(mapIndexes.Select(i => i.Value).ToList());

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
		private static void MapFeatureToMapIndex(IFeature feature, MapType mapType, ref IDictionary<string, MapIndex> mapIndexes)
		{

			// If the feature or mapIndexes is null, then return 
			if (feature == null || mapIndexes == null)
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
				if (!mapIndexes.ContainsKey(pageNumber))
				{
					mapIndexes[pageNumber] = new MapIndex()
					{
						MapType = mapType,
						PageNumber = pageNumber,
						Scale = scale,
						UtmNumber = mgaZone
					};
				}

				// Create the grid reference
				GridReference gridRefObj = MapGridReference(feature);

				// Set the grid reference
				mapIndexes[pageNumber].GridReferences.Add(gridRefObj);

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
		private static GridReference MapGridReference(IFeature feature)
		{
			// Get the grid reference
			string gridReference = feature.DataRow[3].ToString();

			// Get the centre point of the polygon for the grid reference
			IFeature centroid = feature.Centroid();

			// Create the GridReference
			GridReference gridReferenceObj = new GridReference()
			{
				GridSquare = gridReference,
				Latitude = centroid.Coordinates[0].Y,
				Longitude = centroid.Coordinates[0].X

			};

			return gridReferenceObj;

		}

		#endregion

		#region Store in database

		private static void InsertMapIndexes(IList<MapIndex> mapIndexes)
		{

			// Set the items per batch, and determine the amount of batches in totoal
			int itemsPerBatch = 50;
			int batches = ((mapIndexes.Count / itemsPerBatch) + 1);
			int batchesComplete = 0;
			
			// Create the MapIndexRepository
			MapIndexRepository repo = new MapIndexRepository(new FileLogger(), _dbConnectionString);

			// If the -R parameter exists, we need to clear the collection first.
			if (_clearMapIndexes)
			{
				//Task.Run(async () => 
				//{
				//	await 
				//});
				repo.ClearCollection();
			}

			// Loop while batches complete < total number of batches
			while (batchesComplete < batches)
			{

				IList<MapIndex> batchInsert = mapIndexes.Skip(batchesComplete * itemsPerBatch).Take(itemsPerBatch).ToList();

				BatchInsertMapIndexes(batchInsert, repo);
				batchesComplete++;
			}

		}

		private static void BatchInsertMapIndexes(IList<MapIndex> mapIndexes, MapIndexRepository repo)
		{

			// Insert the batch.
			//Task.Run(async () =>
			//{
			//	await 
			//});
			repo.BatchInsert(mapIndexes);
			Thread.Sleep(100);
		}

		#endregion

		#region Command line messages

		/// <summary>
		/// Displays the help the user.
		/// </summary>
		private static void DisplayHelp()
		{
			Console.WriteLine();
			Console.WriteLine("ResponseHub map index parser utility.");
			Console.WriteLine();
			Console.WriteLine("You must specify either the -shp or the -lf options along with the -db option.");
			Console.WriteLine("If you select the -shp option, you must also specify the -map option.");
			Console.WriteLine();
			Console.WriteLine("Command arguments:");
			Console.WriteLine("Options:");
			Console.WriteLine();
			Console.WriteLine("-h\t\t\t\tDisplays this help message.");
			Console.WriteLine();
			Console.WriteLine("-shp <path_to_shapefile>\tPath to a single shapefile to parse.");
			Console.WriteLine("-map\t\t\t\tThe map type. 1 = Spatial Vision, 2 = Melway.");
			Console.WriteLine("-lf <path_to_listfile>\t\tPath to a file that contains the paths to multiple shapefiles to parse. ");
			Console.WriteLine("\t\t\t\tEach shape file should be on its own line.");
			Console.WriteLine("-db <db_conn_string>\t\tConnection string used to connect to the database.");
			Console.WriteLine();
			Console.WriteLine("Options parameters:");
			Console.WriteLine();
			Console.WriteLine("-R\t\t\t\tRemoves any indexes that currently exist in the database");
			Console.WriteLine();

		}

		/// <summary>
		/// Displays the invalid usage message to the user.
		/// </summary>
		private static void DisplayInvalidUsage()
		{
			Console.WriteLine("ResponseHub map index parser - Invalid arguments. Parameters: [-shp <path_to_shapefile> -map [1 = SpatialVision|2 = Melway] | -lf <path_to_list_file>] -db <conn_string>  [-R]");
			Console.WriteLine("Use the option '-h' for further information.");
		}

		#endregion

	}

}
