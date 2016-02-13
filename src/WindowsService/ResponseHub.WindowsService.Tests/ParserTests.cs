﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.WindowsService.Parsers;
using Enivate.ResponseHub.WindowsService.Tests.Fixtures;

namespace Enivate.ResponseHub.WindowsService.Tests
{

	
	public class ParserTests : IClassFixture<UnityCollectionFixture>
	{

		[Trait("Category", "Parser tests")]
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
			MessageParser parser = new MessageParser();
			Message parsedMessage = parser.ParseMessage(pagerMessage);

			// Ensure the job numbers match.
			Assert.Equal(parsedMessage.JobNumber, actualJobNumber, true);

		}

		[Trait("Category", "Parser tests")]
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
			MessageParser parser = new MessageParser();
			Message parsedMessage = parser.ParseMessage(pagerMessage);

			// Ensure the message priority matches
			Assert.Equal(parsedMessage.Priority, actualPriority);
		}

		[Trait("Category", "Parser tests")]
		[Theory(DisplayName = "Can parse pager message - Location information")]
		[InlineData("GLENMORE RD PARWAN SVVB C 6608 E2 (747184) BACC1 CPARW [BACC]", "SVVB C 6608 E2", MapType.SpatialVision, 6608, "E2")]
		[InlineData("RACECOURSE RD SVC 6439 K15 TREE DOWN BLOCKING 1 EAST BOUND LANE [BACC]", "SVC 6439 K15" , MapType.SpatialVision, 6439, "K15")]
		[InlineData("RACECOURSE RD M 316 B4 TREE DOWN BLOCKING 1 EAST BOUND LANE [BACC]", "M 316 B4", MapType.Melway, 316, "B4")]
		public void CanParsePagerMessages_Location(string messageContent, string mapReference, MapType mapType, int mapPage, string gridReference)
		{
			// Create the pager message
			PagerMessage pagerMessage = CreateTestPagerMessage(messageContent);

			// Parse the pager message
			MessageParser parser = new MessageParser();
			Message parsedMessage = parser.ParseMessage(pagerMessage);

			// Ensure the message parses a valid location object
			Assert.NotNull(parsedMessage.Location);
			Assert.Equal(parsedMessage.Location.MapReference, mapReference, true);
			Assert.Equal(parsedMessage.Location.MapType, mapType);
			Assert.Equal(parsedMessage.Location.MapPage, mapPage);
			Assert.Equal(parsedMessage.Location.GridReference, gridReference, true);
		}



		[Trait("Category", "Parser tests")]
		[Theory(DisplayName = "Can parse pager message - Unknown location information")]
		[InlineData("QDThis is a test page. [BACC]")]
		public void CanParsePagerMessages_UnknownLocation(string messageContent)
		{
			// Create the pager message
			PagerMessage pagerMessage = CreateTestPagerMessage(messageContent);

			// Parse the pager message
			MessageParser parser = new MessageParser();
			Message parsedMessage = parser.ParseMessage(pagerMessage);

			// Ensure the Location object is null as we don't have any location information
			Assert.Null(parsedMessage.Location);
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