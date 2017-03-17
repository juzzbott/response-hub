using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using Moq;

using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.PagerDecoder.ApplicationServices.Parsers;
using Enivate.ResponseHub.PagerDecoder.Tests.Fixtures;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Logging;

namespace Enivate.ResponseHub.WindowsService.Tests
{

	
	public class ParserTests : IClassFixture<UnityCollectionFixture>
	{

		[Trait("Category", "Parser tests - Parsed messages")]
		[Theory(DisplayName = "Can parse pager message - Job Numbers")]
		[InlineData("S160132353 SES BACCHUS MARSH TREE DOWN / TRF HAZARD TANYA BARTAM 0432256742 / WESTERN FWY BALLAN /CARTONS RD //RACECOURSE RD SVVB C 6439 K15 TREE DOWN BLOCKING 1 EAST BOUND LANE [BACC]", "S160132353")]
		[InlineData("ALERT F160103773 PARW1 RESCC1 * CAR ACCIDENT - POSS PERSON TRAPPED CNR GEELONG-BACCHUS MARSH RD/GLENMORE RD PARWAN SVC 6608 E2 (747184) BACC1 CPARW [BACC]", "F160103773")]
		[InlineData("ALERT F160107880 ROWS1 RESCC1 GRASS FIRE RESULT OF ACCIDENT BACCHUS MARSH-BALLIANG RD ROWSLEY SVC 6526 A15 (701200) BACC1 CBMSH CROWS [BACC]", "F160107880")]
		[InlineData("This is a test page. [BACC]", "")]
		public void CanParsePagerMessages_JobNumbers(string messageContent, string actualJobNumber)
		{

			// Create the pager message
			PagerMessage pagerMessage = CreateTestPagerMessage(messageContent);

			// Parse the pager message
			JobMessageParser parser = new JobMessageParser(new Mock<IMapIndexRepository>().Object, new Mock<ILogger>().Object);
			JobMessage parsedMessage = parser.ParseMessage(pagerMessage);

			// Ensure the job numbers match.
			Assert.Equal(parsedMessage.JobNumber, actualJobNumber, true);

		}

		[Trait("Category", "Parser tests - Parsed messages")]
		[Theory(DisplayName = "Can parse pager message - Message Priority")]
		[InlineData("@@ALERT F160103773 PARW1 RESCC1 * CAR ACCIDENT - POSS PERSON TRAPPED CNR GEELONG-BACCHUS MARSH RD/GLENMORE RD PARWAN SVC 6608 E2 (747184) BACC1 CPARW [BACC]", MessagePriority.Emergency)]
		[InlineData("HbS160132353 SES BACCHUS MARSH TREE DOWN / TRF HAZARD TANYA BARTAM 0432256742 / WESTERN FWY BALLAN /CARTONS RD //RACECOURSE RD SVVB C 6439 K15 TREE DOWN BLOCKING 1 EAST BOUND LANE [BACC]", MessagePriority.NonEmergency)]
		[InlineData("QDThis is a test page. [BACC]", MessagePriority.Administration)]
		[InlineData("This is a test page. [BACC]", MessagePriority.Administration)]
		public void CanParsePagerMessages_MessagePriority(string messageContent, MessagePriority actualPriority)
		{
			// Create the pager message
			PagerMessage pagerMessage = CreateTestPagerMessage(messageContent);

			// Parse the pager message
			JobMessageParser parser = new JobMessageParser(new Mock<IMapIndexRepository>().Object, new Mock<ILogger>().Object);
			JobMessage parsedMessage = parser.ParseMessage(pagerMessage);

			// Ensure the message priority matches
			Assert.Equal(parsedMessage.Priority, actualPriority);
		}

		[Trait("Category", "Parser tests - Parsed messages")]
		[Theory(DisplayName = "Can parse pager message - Location information")]
		[InlineData("GLENMORE RD PARWAN SVVB C 6608A E2 (747184) BACC1 CPARW [BACC]", "SVVB C 6608A E2 (747184)", MapType.SpatialVision, "6608A", "E2", "747184")]
		[InlineData("RACECOURSE RD SVC 6439 K15 TREE DOWN BLOCKING 1 EAST BOUND LANE [BACC]", "SVC 6439 K15" , MapType.SpatialVision, "6439", "K15", "")]
		[InlineData("RACECOURSE RD M 316 B4 TREE DOWN BLOCKING 1 EAST BOUND LANE [BACC]", "M 316 B4", MapType.Melway, "316", "B4", "")]
		public void CanParsePagerMessages_Location(string messageContent, string mapReference, MapType mapType, string mapPage, string gridReference, string precisionCoordinates)
		{
			// Create the pager message
			PagerMessage pagerMessage = CreateTestPagerMessage(messageContent);

			// Parse the pager message
			JobMessageParser parser = new JobMessageParser(new Mock<IMapIndexRepository>().Object, new Mock<ILogger>().Object);
			JobMessage parsedMessage = parser.ParseMessage(pagerMessage);

			// Ensure the message parses a valid location object
			Assert.NotNull(parsedMessage.Location);
			Assert.Equal(parsedMessage.Location.MapReference, mapReference, true);
			Assert.Equal(parsedMessage.Location.MapType, mapType);
			Assert.Equal(parsedMessage.Location.MapPage, mapPage);
			Assert.Equal(parsedMessage.Location.GridSquare, gridReference, true);
			Assert.Equal(parsedMessage.Location.GridReference, precisionCoordinates, true);
		}
		
		[Trait("Category", "Parser tests - Parsed messages")]
		[Theory(DisplayName = "Can parse pager message - Unknown location information")]
		[InlineData("QDThis is a test page. [BACC]")]
		public void CanParsePagerMessages_UnknownLocation(string messageContent)
		{
			// Create the pager message
			PagerMessage pagerMessage = CreateTestPagerMessage(messageContent);

			// Parse the pager message
			JobMessageParser parser = new JobMessageParser(new Mock<IMapIndexRepository>().Object, new Mock<ILogger>().Object);
			JobMessage parsedMessage = parser.ParseMessage(pagerMessage);

			// Ensure the Location object is null as we don't have any location information
			Assert.Null(parsedMessage.Location);
		}

		[Trait("Category", "Parser tests - Raw pager messages")]
		[Theory(DisplayName = "Can parse raw pager messages")]
		[InlineData("0566760 08:01:00 26-02-16 POCSAG-1  ALPHA   512  @@ALERT F160207784 ALARC1 CADW1 HINDU SOCIETY OF VICTORIA INC ASE - PUMP HOUSE, - INPUT - PUMP HOUSE 52 BOUNDARY RD CARRUM DOWNS /BOUNDARY LANE //OPTIC WAY M 98 F10 (400832) CCADW CFTONS CPATRS [CADW]")]
		[InlineData("0581320 07:58:42 26-02-16 POCSAG-1  ALPHA   512  QDReminder that tonight is pool night come down and have fun with mates all welcome starts around 1800 hours signed BH 042403852")]
		[InlineData("1525176 07:51:41 26-02-16 POCSAG-1  ALPHA   512  HbCORO15 G&SC3 SMOKE ISSUING FROM GRASS BRODERICK-GEELONG RING IN RAMP ON CORIO M 432 G3 (703844) F160207780 CCOROS")]
		[InlineData("0242537 09:33:54 26-02-16 POCSAG-1  ALPHA   512  HbGeelong DO please call Ken Smith SWDO 0417 509 017 re support Corio with line seach 3 - 4 members required. [GEEL]")]
		[InlineData("1398103 09:44:30 26-02-16 POCSAG-3 NUMERIC  512  TONE ONLY")]
		[InlineData("0572104 10:28:02 26-02-16 POCSAG-1  ALPHA   512  QDAnyone helping with the Twilight Night at the Toolern Vale Primary Schoo4?z7w434???(vrhse advise your availability between 4 & 8 pm. Contact Joanne on 0409568603 Thanks Jo [TOOL]")]
		[InlineData("0003227 10:57:07 26-02-16 POCSAG-1  ALPHA   512  @@)&)E51TIMEUPDATE1098000100008000&K0:390:1:0210&&?:")]
		[InlineData("1353287 10:56:13 26-02-16 POCSAG-2 NUMERIC  512  5-*0*163[92-U2868] 2]5580")]
		public void CanParseRawPagerMessage(string rawMessage)
		{

			// Create the pager message parser
			PagerMessageParser parser = new PagerMessageParser(new Mock<ILogger>().Object);
			PagerMessage message = parser.ParsePagerMessage(rawMessage);
			
			// Ensure we have valid message data
			Assert.NotNull(message);
			Assert.False(String.IsNullOrEmpty(message.Address), "Address is null or empty");
			Assert.True(message.Timestamp != DateTime.MinValue, "TimeStamp is min datetime value");
			Assert.True(message.Id != Guid.Empty, "Id is empty guid");
			Assert.True(message.Bitrate > 0, "Bitrate is zero");
			Assert.False(String.IsNullOrEmpty(message.Type), "Type is null or empty");
			Assert.False(String.IsNullOrEmpty(message.Mode), "Mode is null or empty");
			Assert.False(String.IsNullOrEmpty(message.MessageContent), "MessageContent is null or empty");
			Assert.False(String.IsNullOrEmpty(message.ShaHash), "ShaHash is null or empty");
			
		}

		[Trait("Category", "Parser tests - PDW logs")]
		[Theory(DisplayName = "Can determine invalid messages")]
		[InlineData("@@ALERT F160207784 ALARC1 CADW1 HINDU SOCIETY OF VICTORIA INC ASE - PUMP HOUSE, - INPUT - PUMP HOUSE 52 BOUNDARY RD CARRUM DOWNS /BOUNDARY LANE //OPTIC WAY M 98 F10 (400832) CCADW CFTONS CPATRS [CADW]", false)]
		[InlineData("HbCORO15 G&SC3 SMOKE ISSUING FROM GRASS BRODERICK-GEELONG RING IN RAMP ON CORIO M 432 G3 (703844) F160207780 CCOROS", false)]
		[InlineData("QDAnyone helping with the Twilight Night at the Toolern Vale Primary Schoo4?z7w434???(vrhse advise your availability between 4 & 8 pm. Contact Joanne on 0409568603 Thanks Jo [TOOL]", true)]
		[InlineData("5-*0*163[92-U2868] 2]5580", true)]
		[InlineData("G&SC3 SMOKE ISSUING FROM GRASS BRODERICK-GEELONG RING IN RAMP ON CORIO M 432 G3 (703844) F160207780 CCOROS", true)]
		[InlineData("_?hF?()U*U*U*U*U*U*)", true)]
		public void CanDetermineInvalidMessages(string message, bool invalid)
		{

			// Create the parser
			PdwLogFileParser parser = new PdwLogFileParser(new Mock<ILogger>().Object, new Mock<IMapIndexRepository>().Object);
			bool isInvalid = parser.MessageAppearsInvalid(message);

			Assert.True(invalid == isInvalid, "The message does not match the expected invalid value.");
			
		}


		#region Helpers

		/// <summary>
		/// Creates the test pager message based on the message content.
		/// </summary>
		/// <param name="messageContent">The content of the pager message.</param>
		/// <returns>The mock pager message object</returns>
		private static PagerMessage CreateTestPagerMessage(string messageContent)
		{
			return new PagerMessage()
			{
				Address = "00012345",
				Bitrate = 512,
				MessageContent = messageContent,
				Mode = "POCSAG-1",
				ShaHash = "",
				Timestamp = DateTime.UtcNow,
				Type = ""
			};
		}

		#endregion

	}
}
