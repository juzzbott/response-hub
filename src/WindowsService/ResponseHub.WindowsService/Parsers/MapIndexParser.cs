using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.WindowsService.Parsers
{
	public class MapIndexParser
	{

		private string _filePath;

		/// <summary>
		/// Creates a new instance of the MapIndexParser for the index file at 'filename'.
		/// </summary>
		/// <param name="filePath">The path to the index file to parse.</param>
		public MapIndexParser(string filePath)
		{

			if (String.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("The 'filename' parameter must not be empty or contain only white space.");
			}

			if (!File.Exists(filePath))
			{
				throw new IOException(String.Format("Map index file does not exists at path: {0}", filePath));
			}

			// Set the internal file path
			_filePath = filePath;

		}



	}
}
