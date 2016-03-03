using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Enivate.ResponseHub.PagerDecoder.Tests
{


	public class SpatialTests
	{

		[Trait("Category", "Spatial tests")]
		[Theory(DisplayName = "Can get Lat/Lng from Spatial Vision")]
		[InlineData("S160132353 SES BACCHUS MARSH TREE DOWN / TRF HAZARD TANYA BARTAM 0432256742 / WESTERN FWY BALLAN /CARTONS RD //RACECOURSE RD SVVB C 6439 K15 TREE DOWN BLOCKING 1 EAST BOUND LANE [BACC]")]
		[InlineData("ALERT F160103773 PARW1 RESCC1 * CAR ACCIDENT - POSS PERSON TRAPPED CNR GEELONG-BACCHUS MARSH RD/GLENMORE RD PARWAN SVC 6608 E2 (747184) BACC1 CPARW [BACC]")]
		[InlineData("ALERT F160107880 ROWS1 RESCC1 GRASS FIRE RESULT OF ACCIDENT BACCHUS MARSH-BALLIANG RD ROWSLEY SVC 6526 A15 (701200) BACC1 CBMSH CROWS [BACC]")]
		public void CanGetLatLong_FromSpatialVision(string messageContent)
		{

		}

	}
}
