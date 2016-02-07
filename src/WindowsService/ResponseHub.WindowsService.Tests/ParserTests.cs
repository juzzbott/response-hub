using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.WindowsService.Parsers;
using Enivate.ResponseHub.WindowsService.Tests.Fixtures;

namespace Enivate.ResponseHub.WindowsService.Tests
{

	
	public class ParserTests : IClassFixture<UnityCollectionFixture>
	{

		[Theory(DisplayName = "Can parse pager message - Job Numbers")]
		[InlineData("S160132353 SES BACCHUS MARSH TREE DOWN / TRF HAZARD TANYA BARTAM 0432256742 / WESTERN FWY BALLAN /CARTONS RD //RACECOURSE RD SVVB C 6439 K15 TREE DOWN BLOCKING 1 EAST BOUND LANE [BACC]", "S160132353")]
		[InlineData("ALERT F160103773 PARW1 RESCC1 * CAR ACCIDENT - POSS PERSON TRAPPED CNR GEELONG-BACCHUS MARSH RD/GLENMORE RD PARWAN SVC 6608 E2 (747184) BACC1 CPARW [BACC]", "F160103773")]
		[InlineData("ALERT F160107880 ROWS1 RESCC1 GRASS FIRE RESULT OF ACCIDENT BACCHUS MARSH-BALLIANG RD ROWSLEY SVC 6526 A15 (701200) BACC1 CBMSH CROWS [BACC]", "F160107880")]
		[InlineData("This is a test page. [BACC]", "")]
		public void CanParsePagerMessages_JobNumbers(string messageContent, string actualJobNumber)
		{

			PagerMessage pagerMessage = new PagerMessage()
			{
				Address = "00012345",
				Bitrate = 512,
				MessageContent = messageContent,
				Mode = "POCSAG-1",
				ShaHash = "",
				Timestamp = DateTime.UtcNow,
				Type = ""
			};

			// Parse the pager message
			Message parsedMessage = MessageParser.ParseMessage(pagerMessage);

			// Ensure the job numbers match.
			Assert.Equal(parsedMessage.JobNumber, actualJobNumber);

		}

	}
}
