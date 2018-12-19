using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using Enivate.ResponseHub.Tests.Integration.Fixtures;
using Enivate.ResponseHub.DataAccess.MongoDB;

namespace Enivate.ResponseHub.Tests.Integration
{
	public class JobMessageTests : IClassFixture<UnityCollectionFixture>
	{

		[Trait("Integration Tests", "Existing Job Messages")]
		[Fact(DisplayName = "Can get existing messages - By capcode and job number")]
		public void CanGetExistingMessages_ByCapcodeAndJobNumber()
		{

			// Create the list of capcodes and job numbers
			List <KeyValuePair<string, string>> capcodeJobNumbers = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string>("0566888", "F160505373"),
				new KeyValuePair<string, string>("0241033", "S160531933"),
				new KeyValuePair<string, string>("0240577", "S160531629"),
				new KeyValuePair<string, string>("0240433", "S160531872"),
				new KeyValuePair<string, string>("0563000", "F160505123")
			};

			// Get the list of job numbers and ids from the database
			JobMessageRepository repo = new JobMessageRepository();

			// Get the results
			Task t = Task.Run(async () =>
			{
				IList<KeyValuePair<string, Guid>> results = await repo.GetJobMessageIdsFromCapcodeJobNumbers(capcodeJobNumbers);

				// Ensure the results are not null, and there is 5 of them...
				Assert.NotNull(results);
				Assert.Equal(5, results.Count);

			});
			t.Wait();

		}

		[Trait("Integration Tests", "Existing Job Messages")]
		[Fact(DisplayName = "Can get existing messages - By capcode and job number - Non matching records are skipped.")]
		public void CanGetExistingMessages_ByCapcodeAndJobNumber_NonMatchingRecordsAreSkipped()
		{

			// Create the list of capcodes and job numbers
			List<KeyValuePair<string, string>> capcodeJobNumbers = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string>("0566888", "F160505373"),
				new KeyValuePair<string, string>("0566888", "S160531933"),
				new KeyValuePair<string, string>("0566888", "S160531629"),
				new KeyValuePair<string, string>("0566888", "S160531872"),
				new KeyValuePair<string, string>("0566888", "F160505123")
			};

			// Get the list of job numbers and ids from the database
			JobMessageRepository repo = new JobMessageRepository();

			// Get the results
			Task t = Task.Run(async () =>
			{
				IList<KeyValuePair<string, Guid>> results = await repo.GetJobMessageIdsFromCapcodeJobNumbers(capcodeJobNumbers);

				// Ensure the results are not null, and there is 5 of them...
				Assert.NotNull(results);
				Assert.Equal(1, results.Count);

			});
			t.Wait();

		}


	}
}
