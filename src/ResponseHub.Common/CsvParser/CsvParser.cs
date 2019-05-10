using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
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

        public IList<string> ColumnHeaders { get; set; }

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
            ColumnHeaders = new List<string>();
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
            ColumnHeaders = new List<string>();
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
        public DataTable ReadCsvDataTable()
        {

            // Create the list of records
            DataTable table = new DataTable();

            IList<IList<string>> records = ProcessCsvFile();

            // Create the columns in the datatable
            CreateDataTableColumns(ColumnHeaders, ref table);

            // If there is no header loaded, throw exception as we don't have keys to load
            if (!_headerLoaded)
            {
                throw new ApplicationException("There is no header values loaded as the FirstRowHeader parameter is false.");
            }

            // Loop through the records
            foreach (IList<string> record in records)
            {
                // If the record count and column cound is not the same, we don't have the same amount of fields to headers
                if (record.Count != ColumnHeaders.Count)
                {
                    throw new ApplicationException("Unequal number of columns for the specified header columns");
                }

                DataRow row = table.NewRow();

                for (int i = 0; i < record.Count; i++)
                {
                    row[ColumnHeaders[i]] = record[i];
                }

                table.Rows.Add(row);
            }

            return table;

        }

        private void CreateDataTableColumns(IList<string> columnKeys, ref DataTable table)
        {
            // Loop through the column headers and create the data columns
            foreach (string columnHeader in columnKeys)
            {
                DataColumn column = new DataColumn()
                {
                    DataType = Type.GetType("System.String"),
                    ColumnName = columnHeader,
                    AutoIncrement = false,
                    Caption = columnHeader,
                    ReadOnly = false,
                    Unique = false,
                };
                table.Columns.Add(column);
            }
        }

        private IList<IList<string>> ProcessCsvFile()
        {

            IList<IList<string>> records = new List<IList<string>>();

            foreach (string line in _csvDataLines)
            {
                // If the line starts with '#' it's a comment so skip it
                if (String.IsNullOrEmpty(line) || line[0] == '#')
                {
                    continue;
                }

                // Process the record
                IList<string> record = ProcessCsvLine(line);

                if (_firstRowHeader && !_headerLoaded)
                {
                    ColumnHeaders = record;
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
            StringBuilder value = new StringBuilder();
            bool quoteState = false;

            // Loop through each char in the string
            for(int i = 0; i < csvLine.Length; i++)
            {

                // If the char is a ", flip the quoteToken state
                if (csvLine[i] == '"')
                {
                    quoteState = !quoteState;
                    continue;
                }

                if (csvLine[i] == ',' && !quoteState)
                {
                    lineParts.Add(value.ToString());
                    value.Clear();
                    continue;
                }

                // Append the value
                value.Append(csvLine[i]);
            }

            // Add the final value
            lineParts.Add(value.ToString());
            

            return lineParts;

        }
    }
}
