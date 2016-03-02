using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DotSpatial.Data;

using MongoDB.Driver;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.MapIndexParser.Parsers;
using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.DataAccess.MongoDB;
using Enivate.ResponseHub.Logging;

namespace Enivate.ResponseHub.MapIndexParser
{
	class Program
	{

		private static string _dbConnectionString;

		private static MapIndexRepository _mapIndexRepository;
		
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

				// Initialise the import
				InitialiseImport(args);

				MapType mapType = GetMapTypeFromArgument(args);

				// If the map type is unknown, then show the usage information
				if (mapType == MapType.Unknown)
				{
					DisplayInvalidUsage();
					return;
				}

				// Process only a single shapefile
				string shapeFilePath = Path.GetFullPath(args[1]);
				SpatialVisionParser parser = new SpatialVisionParser();
				parser.ProcessShapeFile(shapeFilePath, mapType);

				// Insert the map indexes
				InsertMapIndexes(parser.MapIndexes.Select(i => i.Value).ToList());
			}
			else if (args[0].ToLower() == "-lf")
			{

				// Initialise the import
				InitialiseImport(args);

				// Process only a single shapefile
				string shapeFileListPath = Path.GetFullPath(args[1]);

				// Process the list of shape files.
				SpatialVisionParser parser = new SpatialVisionParser();
				parser.ProcessShapeFileList(shapeFileListPath);

				// Insert the map indexes
				InsertMapIndexes(parser.MapIndexes.Select(i => i.Value).ToList());
			}
			else if (args[0].ToLower() == "-melway")
			{

				// Initialise the import
				InitialiseImport(args);

				DateTime startDate = DateTime.Now;

				// Parse the melways indexes
				MelwayParser parser = new MelwayParser();
				parser.GetMapIndexes();
				//parser.DummyMapIndexes();
				
				// Insert the map indexes
				InsertMapIndexes(parser.MapIndexes.Select(i => i.Value).ToList());

				TimeSpan timeTaken = (DateTime.Now - startDate);
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine("Melway import complete.");
				Console.WriteLine(String.Format("Duration: {0}", timeTaken));
				Console.WriteLine(String.Format("Total map pages: {0}", parser.MapIndexes.Count));
				Console.WriteLine(String.Format("Total grid references: {0}", parser.MapIndexes.Select(i => i.Value).Sum(i => i.GridReferences.Count)));

			}
			else
			{
				// Display invalid usage
				DisplayInvalidUsage();
			}


		}

		private static void InitialiseImport(string[] args)
		{
			// Ensure -db option exists
			if (!EnsureDbConnectionString(args))
			{
				DisplayInvalidUsage();
				return;
			}

			// Instantiate the mongo repo.
			_mapIndexRepository = new MapIndexRepository(new FileLogger(), _dbConnectionString);

			// Check if we need to clear the indexes first.
			if (args.Contains("-R"))
			{
				Task.Run(async () =>
				{
					await _mapIndexRepository.ClearCollection();
				});
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
		
		#region Store in database

		private static void InsertMapIndexes(IList<MapIndex> mapIndexes)
		{

			// If there are no map indexes, then return
			if (mapIndexes == null || mapIndexes.Count == 0)
			{
				return;
			}

			// Set the items per batch, and determine the amount of batches in totoal
			int itemsPerBatch = 50;
			int batches = ((mapIndexes.Count / itemsPerBatch) + 1);
			int batchesComplete = 0;
			

			// Loop while batches complete < total number of batches
			while (batchesComplete < batches)
			{

				IList<MapIndex> batchInsert = mapIndexes.Skip(batchesComplete * itemsPerBatch).Take(itemsPerBatch).ToList();

				BatchInsertMapIndexes(batchInsert);
				batchesComplete++;
			}

		}

		private static void BatchInsertMapIndexes(IList<MapIndex> mapIndexes)
		{
			try
			{

				_mapIndexRepository.BatchInsert(mapIndexes);
				Thread.Sleep(100);
			}
			catch (Exception ex)
			{
				int i = 0;
			}
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
			Console.WriteLine("-melway\t\t\t Parses the Melway services for the map indexes.");
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
			Console.WriteLine("ResponseHub map index parser - Invalid arguments. Parameters: -shp <path_to_shapefile> -map [1 = SpatialVision] | -lf <path_to_list_file> | -melway -db <conn_string>  [-R]");
			Console.WriteLine("Use the option '-h' for further information.");
		}

		#endregion

	}

}
