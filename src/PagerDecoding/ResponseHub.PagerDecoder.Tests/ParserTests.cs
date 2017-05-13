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
using System.Diagnostics;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Addresses.Interface;
using System.IO;

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
			JobMessageParser parser = new JobMessageParser(new Mock<IAddressService>().Object, new Mock<IMapIndexRepository>().Object, new Mock<ILogger>().Object);
			JobMessage parsedMessage = parser.ParseMessage(pagerMessage).Result;

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
			JobMessageParser parser = new JobMessageParser(new Mock<IAddressService>().Object, new Mock<IMapIndexRepository>().Object, new Mock<ILogger>().Object);
			JobMessage parsedMessage = parser.ParseMessage(pagerMessage).Result;

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
			JobMessageParser parser = new JobMessageParser(new Mock<IAddressService>().Object, new Mock<IMapIndexRepository>().Object, new Mock<ILogger>().Object);
			JobMessage parsedMessage = parser.ParseMessage(pagerMessage).Result;

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
			JobMessageParser parser = new JobMessageParser(new Mock<IAddressService>().Object, new Mock<IMapIndexRepository>().Object, new Mock<ILogger>().Object);
			JobMessage parsedMessage = parser.ParseMessage(pagerMessage).Result;

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
			PdwLogFileParser parser = new PdwLogFileParser(new Mock<ILogger>().Object, new Mock<IMapIndexRepository>().Object, new Mock<IDecoderStatusRepository>().Object, new Mock<IJobMessageService>().Object, new Mock<IAddressService>().Object);
			bool isInvalid = parser.MessageAppearsInvalid(message);

			Assert.True(invalid == isInvalid, "The message does not match the expected invalid value.");
			
		}

		[Trait("Category", "Parser tests - Addresses")]
		[Theory(DisplayName = "Can get address from message")]
		[InlineData("ALERT F170208772 COIM1 RESCC1 * CAR ACCIDENT - POSS PERSON TRAPPED 1979 GISBORNE-BACCHUS MARSH RD COIMADAI /NUGGETTY TRK //LEBREX RD SVC 6442 F13 (753375) BACC1 CBMSH CCOIM [BACC]", "1979 GISBORNE-BACCHUS MARSH RD COIMADAI")]
		[InlineData("S170231491 BACC - TREE DOWN - TREE DOWN IN DRIVEWAY OF REHAB CENTRE - 515 CAMERONS RD COIMADAI /WHITE LANE //SEEREYS TRK SVC 6442 D14 (734361) SYLVANNA - STAFF 53674399 [BACC]", "515 CAMERONS RD COIMADAI")]
		[InlineData("S170231278 BACC - BUILDING DAMAGE - BUILDING DAMAGE - 107 FLANAGANS DR MERRIMU M 334 J6 (780272) LOKI DEVEREAUX 0418738402 [BACC]", "107 FLANAGANS DR MERRIMU")]
		[InlineData("S170231184 BACC - FLOOD - BURST WATER MAIN - 1 / 12 INGLIS ST MADDINGLEY /GRIFFITH ST //LABILLIERE ST M 333 F9 (730259) BILL WADESON 0429646365 [BACC]", "1 / 12 INGLIS ST MADDINGLEY")]
		[InlineData("ALERT F170203470 BMSH2 RESCC1 * CAR ACCIDENT - POSS PERSON TRAPPED TOP OF PENTLAND HILLS WESTERN FWY BACCHUS MARSH M 333 H3 (735284) BACC1 CBMSH CMYRN [BACC]", "TOP OF PENTLAND HILLS WESTERN FWY BACCHUS MARSH")]
		[InlineData("S170230990 BACC - TREE DOWN / TRF HAZARD - TREE COVERING WHOLE ROAD - BALLAN-EGERTON RD MOUNT EGERTON SVC 8188 F6 (447311) LEE RYALL 0458956089 [BACC]", "BALLAN-EGERTON RD MOUNT EGERTON")]
		[InlineData("S170230973 BACC - INCIDENT - OTHER - PENTLAND PRIMARY SCHOOL - DARLEY 164 HALLETTS WAY DARLEY /WITTICK ST //DURHAM ST M 333 E1 (726292) KAREN BERTON 0353676080 [BACC]", "164 HALLETTS WAY DARLEY")]
		[InlineData("S170230807 BACC - FLOOD - BURST WATER MAIN FLOODING YARD AND GARAGE - 4 / 36 INGLIS ST MADDINGLEY /LABILLIERE ST //BACCHUS ST M 333 F8 (729263) SHARON COOK 0419513112 [BACC]", "4 / 36 INGLIS ST MADDINGLEY")]
		[InlineData("S170330677 WTLS - FLOOD - ROADWAY FLOODING - WOOLWORTHS - SAFEWAY - BUNDOORA - PLENTY RD 69 PLENTY RD BUNDOORA /DALY PL //MCLEANS RD M 9 J12 (289262) MARGARET 0449908849 [WTLS]", "69 PLENTY RD BUNDOORA")]
		[InlineData("CARO4 NOSTC1 CAR FIRE CNR WELLINGTON DR/BELLEVUE BVD HILLSIDE (GREATER MELBOURNE) M 354 F9 (002265) F170306440 CCARO FGD11 [CARO]", "CNR WELLINGTON DR/BELLEVUE BVD HILLSIDE")]
		[InlineData("WARB1 STRUC1 HOUSE FIRE 8 ELLIS CT WARBURTON /YUONGA RD M 290 E3 (859212) F170306435 CWARB CWBRN", "8 ELLIS CT WARBURTON")]
		[InlineData("PKHM2 INCIC1 SMELL OF GAS 6 COLONIAL WAY PAKENHAM /HERITAGE BVD //MCGREGOR RD M 317 B9 (660840) F170306414 COFFI CPKHM", "6 COLONIAL WAY PAKENHAM")]
		[InlineData("BELM10 STRUC1 HOUSE FIRE 38 REGENT ST BELMONT /CHURCH ST //THOMSON ST M 451 H10 (672717) F170306409 CBELM CGONGS CHIGH", "38 REGENT ST BELMONT")]
		[InlineData("S170330676 NBIK - TREE DOWN / TRF HAZARD - TREE DOWN OVER ROAD - CNR DIAMOND CREEK RD/PLENTY RIVER DR GREENSBOROUGH M 11 C8 (341276) BEN DARMANIM 0402826527 [NBIK]", "CNR DIAMOND CREEK RD/PLENTY RIVER DR GREENSBOROUGH")]
		[InlineData("ALERT F170306406 CONU2 G&SC1 GRASS FIRE TREGILGAS RD COLAC COLAC SVNE 268 D4 (763920) CCONU CCUWA CNARI [NARI]", "TREGILGAS RD COLAC COLAC")]
		[InlineData("ALERT F170306405 VTWN7 ALARC1 VIOLET TOWN BUSH NURSING CTRE INPUT - FRONT FOYER ENT RHS COWSLIP ST VIOLET TOWN /ROSE ST SV8407 D6 (849447) CBALM CEURO CVTWN [VTWN]", "FRONT FOYER ENT RHS COWSLIP ST VIOLET TOWN")]
		[InlineData("BEAC4 INCIC3 WASHAWAY RESULT OF ACCIDENT CNR OLD PRINCES HWY/PRINCES HWY BEACONSFIELD M 214 B2 (580868) F170306363 CBEAC", "CNR OLD PRINCES HWY/PRINCES HWY BEACONSFIELD")]
		[InlineData("ALERT F170306396 REDC3 STRUC1 CREW TO STANDBY AT STATION. REDCT2 MAY BE REQUIRED FOR ACCESS FOR CAMPER FIRE DAWSONS TRK RED CLIFFS SVNW 4364 D12 (239986) CREDC [REDC]", "CREW TO STANDBY AT STATION. REDCT2 MAY BE REQUIRED FOR ACCESS FOR CAMPER FIRE DAWSONS TRK RED CLIFFS")]
		[InlineData("\"ALERT F160712223 ALARC1 SHEP2C SHEPP VILLAGES - ACACIA LODGE ASE - INSIDE FIP FRONT FOYER ACACIA, INPUT - FRONT FOYER HAKEA LODGE 9 BATMAN AV SHEPPARTON /TARCOOLA DR //THE BOULEVARD - SV8385 E6 (557744) CMPNA CSEST CSHEP [SHEP]\"", "9 BATMAN AV SHEPPARTON")]
		[InlineData("ALERT F160810936 MOEE1 NOSTC3 BINS ON FIRE OUT THE BACK OF HOME HARDWARE HOME TM AND H TIMBER AND HARDWARE - MOE - GEORGE ST 56 GEORGE ST MOE /PURVIS LANE //SAVIGES RD SVSE 8561 G7 (351743) CMOEE [MOEE]", "56 GEORGE ST MOE")]
		public void CanGetAddressFromMessage(string message, string expectedAddressValue)
		{

			// Create the address parser and get the address string from it
			AddressParser parser = new AddressParser();
			string addressValue = parser.GetAddressFromMessage(message);

			// Ensure we get some address value back from the parser
			Assert.False(String.IsNullOrEmpty(addressValue), "Empty or null address returned from the parser.");

			// Ensure the address is the expected address value
			Assert.Equal(expectedAddressValue, addressValue);

		}

		[Trait("Category", "Parser tests - HTML source")]
		[Fact(DisplayName = "Can parse pager messages from HTML")]
		public void CanParsePagerMessagesFromHtml()
		{

			// Get the html from the file
			string html = File.ReadAllText(String.Format("{0}\\mazzonet_html.txt", Environment.CurrentDirectory));

			// Create the Mazzonet Web Parser
			MazzanetWebParser parser = new MazzanetWebParser(new Mock<ILogger>().Object, new Mock<IMapIndexRepository>().Object, new Mock<IDecoderStatusRepository>().Object, new Mock<IJobMessageService>().Object, new Mock<IAddressService>().Object);

			// Parse the html into pager messages
			IList<PagerMessage> messages = parser.ParsePagerMessagesFromHtml(html);

			Assert.NotNull(messages);
			Assert.True(messages.Count > 0);

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
