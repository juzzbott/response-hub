using Enivate.ResponseHub.Common.CsvParser;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Enivate.ResponseHub.Tests.Unit
{
    public class CsvParserTests
    {

        [Theory(DisplayName = "Can parse CSV data - Single line.")]
        [InlineData("Email,FirstName,Last Name,MemberNo,Access Type", "Email", "FirstName", "Last Name", "MemberNo", "Access Type", 5)]
        [InlineData("\"test_user_1@email.com\",\"Test\",\"User\",\"65001\",\"General user\"", "test_user_1@email.com", "Test", "User", "65001", "General user", 5)]
        [InlineData("\"\",\"\",\"\",\"\",\"\"", "", "", "", "", "", 5)]
        [InlineData(",,,,", "", "", "", "", "", 5)]
        [InlineData("Email,,Last Name,MemberNo,", "Email", "", "Last Name", "MemberNo", "", 5)]
        [Trait("Category", "Csv Parser Tests")]
        public void CanParseCsvData_SingleLine(string csvData, string val1, string val2, string val3, string val4, string val5, int totalExpectedValues)
        {

            // Create the parser
            CsvParser csvParser = new CsvParser(csvData, false);
            IList<IList<string>> parsedData = csvParser.ReadCsvData();

            // Ensure there is 1 record
            Assert.True(parsedData.Count == 1, "No CSV records found in string parsed.");

            // Ensure there are the expected number of entries
            Assert.Equal(totalExpectedValues, parsedData[0].Count);

            // Ensure the values match
            Assert.True(parsedData[0][0].Equals(val1, StringComparison.CurrentCultureIgnoreCase));
            Assert.True(parsedData[0][1].Equals(val2, StringComparison.CurrentCultureIgnoreCase));
            Assert.True(parsedData[0][2].Equals(val3, StringComparison.CurrentCultureIgnoreCase));
            Assert.True(parsedData[0][3].Equals(val4, StringComparison.CurrentCultureIgnoreCase));
            Assert.True(parsedData[0][4].Equals(val5, StringComparison.CurrentCultureIgnoreCase));

        }

        [Theory(DisplayName = "Can parse CSV data - CSV Files.")]
        [InlineData("App_Data\\csv_example_headers.csv", true, 6)]
        [Trait("Category", "Csv Parser Tests")]
        public void CanParseCsvData_CsvFile(string csvFile, bool header, int totalDataRows)
        {

            // Create the parser
            CsvParser csvParser = new CsvParser(new FileStream(csvFile, FileMode.Open), header);
            DataTable parsedData = csvParser.ReadCsvDataTable();

            // Ensure there is 1 record
            Assert.True(parsedData.Rows.Count == totalDataRows, "Unexpected number of rows in the datafile.");

        }

    }
}
