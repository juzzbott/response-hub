using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using Enivate.ResponseHub.ApplicationServices.Wrappers;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Addresses;

using Moq;

namespace Enivate.ResponseHub.Tests.Unit
{
	public class GeocodingTests
	{



		[Trait("Category", "Parser tests - Google Geocoding")]
		[Theory(DisplayName = "Can decode Google Geocode result")]
		[InlineData("{\"results\":[{\"address_components\":[{\"long_name\":\"208\",\"short_name\":\"208\",\"types\":[\"street_number\"]},{\"long_name\":\"Swifts Creek East Road\",\"short_name\":\"Swifts Creek E Rd\",\"types\":[\"route\"]},{\"long_name\":\"Swifts Creek\",\"short_name\":\"Swifts Creek\",\"types\":[\"locality\",\"political\"]},{\"long_name\":\"East Gippsland Shire\",\"short_name\":\"East Gippsland\",\"types\":[\"administrative_area_level_2\",\"political\"]},{\"long_name\":\"Victoria\",\"short_name\":\"VIC\",\"types\":[\"administrative_area_level_1\",\"political\"]},{\"long_name\":\"Australia\",\"short_name\":\"AU\",\"types\":[\"country\",\"political\"]},{\"long_name\":\"3896\",\"short_name\":\"3896\",\"types\":[\"postal_code\"]}],\"formatted_address\":\"208 Swifts Creek E Rd, Swifts Creek VIC 3896, Australia\",\"geometry\":{\"location\":{\"lat\":-37.2952032,\"lng\":147.7490779},\"location_type\":\"ROOFTOP\",\"viewport\":{\"northeast\":{\"lat\":-37.2938542197085,\"lng\":147.7504268802915},\"southwest\":{\"lat\":-37.2965521802915,\"lng\":147.7477289197085}}},\"place_id\":\"ChIJIaLL6Q4EJWsRBb5w2900-fQ\",\"types\":[\"street_address\"]}],\"status\":\"OK\"}")]
		public void CanDecodeGoogleGeocodeResult(string jsonResult)
		{
			// Create the address parser and get the address object from it
			GoogleGeocodeWrapper wrapper = new GoogleGeocodeWrapper(new Mock<ILogger>().Object);
			StructuredAddress result = wrapper.GetStructuredAddressFromGoogleGeocode(jsonResult, "skdjfljkadflkajdfklajdflkdjf");

			Assert.NotNull(result);
		}

	}
}
