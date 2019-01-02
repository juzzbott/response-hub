using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Common.CsvParser
{
    public class CsvParser
    {

        /// <summary>
        /// The variable the holds the CSV data
        /// </summary>
        private IList<string> _csvDataLines;

        private bool _firstRowHeader = false;

        private bool _headerLoaded = false;

        private const string RegexCsvPattern = "^((\"(?:[^\"]|\"\")*\" |[^,] *)(, (\"(?:[^\"]| \"\")*\"|[^,]*))*)$";

        public IList<string> ColumnKeys { get; set; }

        /// <summary>
        /// Creates a new instance of the CsvParser with the stream to read the CSV data from.
        /// </summary>
        /// <param name="stream">The stream to read the data from</param>
        public CsvParser(Stream stream, bool firstRowHeader)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string csvData = reader.ReadToEnd();
                if (csvData == null)
                {
                    csvData = "";
                }
                _csvDataLines = csvData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            }

            _firstRowHeader = firstRowHeader;

            // Instantiate the column keys
            ColumnKeys = new List<string>();
        }

        /// <summary>
        /// Creates a new instance of the CsvParser with the string of CSV data.
        /// </summary>
        /// <param name="csvData">The CSV data to parse</param>
        public CsvParser(string csvData, bool firstRowHeader)
        {
            if (csvData == null)
            {
                csvData = "";
            }
            _csvDataLines = csvData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            _firstRowHeader = firstRowHeader;

            // Instantiate the column keys
            ColumnKeys = new List<string>();
        }

        /// <summary>
        /// Read the CSV data into the list of records
        /// </summary>
        /// <returns></returns>
        public IList<IList<string>> ReadCsvData()
        {
            return ProcessCsvFile();
        }

        /// <summary>
        /// Read the CSV data into the list of keyed records.
        /// </summary>
        /// <returns></returns>
        public IList<IList<KeyValuePair<string, string>>> ReadCsvDataWithKeys()
        {

            // Create the list of records
            IList<IList<KeyValuePair<string, string>>> keyRecords = new List<IList<KeyValuePair<string, string>>>();

            IList<IList<string>> records = ProcessCsvFile();

            // If there is no header loaded, throw exception as we don't have keys to load
            if (!_headerLoaded)
            {
                throw new ApplicationException("There is no header values loaded as the FirstRowHeader parameter is false.");
            }

            // Loop through the records
            foreach (IList<string> record in records)
            {
                // If the record count and column cound is not the same, we don't have the same amount of fields to headers
                if (record.Count != ColumnKeys.Count)
                {
                    throw new ApplicationException("Unequal number of columns for the specified header columns");
                }

                IList<KeyValuePair<string, string>> keyRecord = new List<KeyValuePair<string, string>>();

                for (int i = 0; i < record.Count; i++)
                {
                    keyRecord.Add(new KeyValuePair<string, string>(ColumnKeys[i], record[i]));
                }

                keyRecords.Add(keyRecord);
            }

            return keyRecords;

        }

        private IList<IList<string>> ProcessCsvFile()
        {

            IList<IList<string>> records = new List<IList<string>>();

            foreach (string line in _csvDataLines)
            {
                // If the line starts with '#' it's a comment so skip it
                if (line[0] == '#')
                {
                    continue;
                }

                // Process the record
                IList<string> record = ProcessCsvLine(line);

                if (_firstRowHeader && !_headerLoaded)
                {
                    ColumnKeys = record;
                    _headerLoaded = true;
                }
                else
                {
                    records.Add(record);
                }
            }

            return records;
        }

        /// <summary>
        /// Process the csv line. 
        /// </summary>
        /// <param name="csvLine"></param>
        /// <returns></returns>
        private IList<string> ProcessCsvLine(string csvLine)
        {

            IList<string> lineParts = new List<string>();

            // Loop through each string
            foreach(string part in Regex.Split(csvLine, RegexCsvPattern))
            {

                string linePart = part.Trim();

                // If the line starts with " and ends with " get the substring of 1 char from each end so that the " are removed from start and end
                if (linePart.StartsWith("\"") && linePart.EndsWith("\""))
                {
                    linePart = linePart.Substring(1, (linePart.Length - 1));
                }

                lineParts.Add(linePart);
            }

            return lineParts;

        }
    }
}
