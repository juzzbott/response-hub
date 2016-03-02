using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Enivate.ResponseHub.Model.Spatial;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enivate.ResponseHub.MapIndexParser.Parsers
{
	public class MelwayParser : IMapIndexParser
	{

		private readonly char[] _pageXList = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K' };
		private readonly int[] _pageYList = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
		private IList<KeyValuePair<int, int>> _mapPageSets;
		private string _webUrlFormat = "http://www.street-directory.com.au/sd3_new/ws/search_by_mapref.php?book=Melway&page={0}&grid={1}{2}";

		public IDictionary<string, MapIndex> MapIndexes { get; set; }

		public MelwayParser()
		{
			_mapPageSets = new List<KeyValuePair<int, int>>();
			_mapPageSets.Add(new KeyValuePair<int, int>(3, 395));
			_mapPageSets.Add(new KeyValuePair<int, int>(615, 697));

			// Instantiate the map indexes.
			MapIndexes = new Dictionary<string, MapIndex>();
		}

		public void GetMapIndexes()
		{

			// Iterate through each set of pages
			foreach(KeyValuePair<int, int> set in _mapPageSets)
			{

				// Get the indexes in the set of pages.
				GetSetIndexes(set.Key, set.Value);

			}

			// Validate the map indexes are valid and not all containing nulls. 
			ValidateMapIndexes();
			
		}

		private void ValidateMapIndexes()
		{

			IDictionary<string, MapIndex> validMapIndexes = new Dictionary<string, MapIndex>();

			foreach(KeyValuePair<string, MapIndex> mapIndex in MapIndexes)
			{
				if (mapIndex.Value.GridReferences.Count == 0)
				{
					// All null, so don't add it
					continue;
				}
				
				// Add the map index to the valid list.
				validMapIndexes.Add(mapIndex.Key, mapIndex.Value);
				
			}

			// Set the MapIndexes to the ValidMapIndexes
			MapIndexes = validMapIndexes;
		}

		/// <summary>
		/// Gets the indexes for the specific set of indexes.
		/// </summary>
		/// <param name="minPage"></param>
		/// <param name="maxPage"></param>
		private void GetSetIndexes(int minPage, int maxPage)
		{
			// Loop through each page in the set
			for (int i = minPage; i < (maxPage + 1); i++)
			{

				GetPageIndexes(i.ToString());

			}
		}

		/// <summary>
		/// Gets the indexes on a specific page.
		/// </summary>
		/// <param name="pageNumber"></param>
		private void GetPageIndexes(string pageNumber)
		{

			// If the page number doesn't exist in the dictionary of map indexes, create it
			if (!MapIndexes.ContainsKey(pageNumber)) 
			{
				MapIndexes[pageNumber] = new MapIndex()
				{
					MapType = MapType.Melway,
					PageNumber = pageNumber.ToString(),
					Scale = 20000,
					UtmNumber = -1
				};
			}

			// Create the task block.
			//var taskBlock = new ActionBlock<Tuple<string, char, int>>(_ => GetSingleIndexFromWeb(_), new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 10 });

			// Loop through the X list
			foreach (char x in _pageXList)
			{
				// Loop through the Y list
				foreach(int y in _pageYList)
				{
					Tuple<string, char, int> gridData = new Tuple<string, char, int>(pageNumber, x, y);
					Task.Run(() => GetSingleIndexFromWeb(gridData)).Wait();
					//taskBlock.Post(gridData);
				}
			}

			//taskBlock.Complete(); //Signal completion
			//await taskBlock.Completion; // Async await for completion.

		}

		/// <summary>
		/// Gets the index information from the web service.
		/// </summary>
		/// <param name="pageNumber">The page number for the index</param>
		/// <param name="x">The X (A - K) value</param>
		/// <param name="y">The Y (1 - 12) value.</param>
		private async Task GetSingleIndexFromWeb(Tuple<string, char, int> gridData)
		{

			// Build the URL
			string url = String.Format(_webUrlFormat, gridData.Item1, gridData.Item2, gridData.Item3);

			// Build the web request
			HttpWebRequest request = WebRequest.CreateHttp(url);
			request.Accept = "application/json, text/javascript, */*; q=0.01";
			request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.109 Safari/537.36";
			request.Referer = "http://www.street-directory.com.au/act/computer-software-packages";
			request.Headers.Add("X-Requested-With", "XMLHttpRequest");
			request.Headers.Add("Accept-Encoding", "gzip, deflate, sdch");
			request.Headers.Add("Accept-Language", "en-US,en;q=0.8");

			try {

				// Get the response
				HttpWebResponse response = ((HttpWebResponse)await request.GetResponseAsync());

				// If the response is not 200 OK, then show error and return
				if (response.StatusCode != HttpStatusCode.OK)
				{
					// Write the error information
					Console.WriteLine(String.Format("Url '{0}' returned status code: {1}", url, response.StatusDescription));
				}

				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{

					// Create the GridReference from the JsonData
					GridReference gridRef = ParseJsonResponse(reader.ReadToEnd());

					if (gridRef == null)
					{
						Console.WriteLine(String.Format("Skipping non-existant grid reference. Map page: {0} Grid reference: {1}{2}", gridData.Item1, gridData.Item2, gridData.Item3));
						return;
					}

					// Add the grid reference to the list of grid references in the map index.
					MapIndexes[gridData.Item1].GridReferences.Add(gridRef);

					// Show indication the parsing it complete.
					Console.WriteLine(String.Format("Parsed map page: {0} Grid reference: {1}{2}", gridData.Item1, gridData.Item2, gridData.Item3));
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}


		}

		private GridReference ParseJsonResponse(string jsonResponse)
		{

			// Format of the response string: 
			// {"Result":1,"Book":"Melway Greater Melbourne","Page":"327","Grid":"G10","Lon":"144.430101500000","Lat":"-37.644326500000","t":"-37.64248500","l":"144.42787800","b":"-37.64616800","r":"144.43232500"}

			// Get the response object.
			dynamic response = JObject.Parse(jsonResponse);

			// If there is no successfull result, then just return null.
			if (response.Result == 0)
			{
				return null;
			}

			// Create the grid reference.
			GridReference gridRef = new GridReference()
			{
				GridSquare = response.Grid,
				Latitude = Double.Parse(response.Lat.ToString()),
				Longitude = Double.Parse(response.Lon.ToString())
			};

			// return the grid reference
			return gridRef;
		}
	}
}
