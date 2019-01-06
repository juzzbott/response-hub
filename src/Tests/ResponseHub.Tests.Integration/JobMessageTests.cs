using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using Enivate.ResponseHub.Tests.Integration.Fixtures;
using Enivate.ResponseHub.DataAccess.MongoDB;
using Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers;
using Enivate.ResponseHub.Model.Addresses.Interface;
using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Messages;

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

        [Trait("Integration Tests", "Generate JobMessage object")]
        [Theory(DisplayName = "Can generate job message - From pager message.")]
        [InlineData("0241249", "S190150219 BACC - BUILDING DAMAGE - TILES OFF ROOF BUILDING DAMAGE - 5 KEITH CT DARLEY /SILVERDALE DR M 333 F3 (727285) LEO GLEDEGEVQURE 0466153217 [BACC]")]
        public async void CanGenerateJobMessage_FromPagerMessage(string capcode, string rawMessageString)
        {

            IAddressService addressService = ServiceLocator.Get<IAddressService>();
            IMapIndexRepository mapIndexRepo = ServiceLocator.Get<IMapIndexRepository>();
            ILogger logger = ServiceLocator.Get<ILogger>();

            PagerMessageParser pagerParser = new PagerMessageParser(logger);
            string rawMessage = String.Format("{0} {1} POCSAG-1  ALPHA   512  {2}", capcode, DateTime.Now.ToString("HH:mm:ss dd-MM-yy"), rawMessageString);
            PagerMessage pagerMessage = pagerParser.ParsePagerMessage(rawMessage);

            JobMessageParser parser = new JobMessageParser(addressService, mapIndexRepo, logger);
            JobMessage message = await parser.ParseMessage(pagerMessage);

            Assert.NotNull(message);

        }


	}
}
