using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using Enivate.ResponseHub.Wrappers.Google;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Addresses;

using Moq;

namespace Enivate.ResponseHub.Tests.Unit
{
	public class GeocodingTests
	{

		[Trait("Category", "Parser tests - Google Geocoding")]
		[Theory(DisplayName = "Can decode Google Geocode result")]
		[InlineData("{\"results\":[{\"address_components\":[{\"long_name\":\"1\",\"short_name\":\"1\",\"types\":[\"subpremise\"]},{\"long_name\":\"208\",\"short_name\":\"208\",\"types\":[\"street_number\"]},{\"long_name\":\"Swifts Creek East Road\",\"short_name\":\"Swifts Creek E Rd\",\"types\":[\"route\"]},{\"long_name\":\"Swifts Creek\",\"short_name\":\"Swifts Creek\",\"types\":[\"locality\",\"political\"]},{\"long_name\":\"East Gippsland Shire\",\"short_name\":\"East Gippsland\",\"types\":[\"administrative_area_level_2\",\"political\"]},{\"long_name\":\"Victoria\",\"short_name\":\"VIC\",\"types\":[\"administrative_area_level_1\",\"political\"]},{\"long_name\":\"Australia\",\"short_name\":\"AU\",\"types\":[\"country\",\"political\"]},{\"long_name\":\"3896\",\"short_name\":\"3896\",\"types\":[\"postal_code\"]}],\"formatted_address\":\"208 Swifts Creek E Rd, Swifts Creek VIC 3896, Australia\",\"geometry\":{\"location\":{\"lat\":-37.2952032,\"lng\":147.7490779},\"location_type\":\"ROOFTOP\",\"viewport\":{\"northeast\":{\"lat\":-37.2938542197085,\"lng\":147.7504268802915},\"southwest\":{\"lat\":-37.2965521802915,\"lng\":147.7477289197085}}},\"place_id\":\"ChIJIaLL6Q4EJWsRBb5w2900-fQ\",\"types\":[\"street_address\"]}],\"status\":\"OK\"}")]
		[InlineData("{\"results\":[{\"address_components\":[{\"long_name\":\"Leviathan Road\",\"short_name\":\"Leviathan Rd\",\"types\":[\"route\"]},{\"long_name\":\"Stawell\",\"short_name\":\"Stawell\",\"types\":[\"locality\",\"political\"]},{\"long_name\":\"Northern Grampians Shire\",\"short_name\":\"Northern Grampians\",\"types\":[\"administrative_area_level_2\",\"political\"]},{\"long_name\":\"Victoria\",\"short_name\":\"VIC\",\"types\":[\"administrative_area_level_1\",\"political\"]},{\"long_name\":\"Australia\",\"short_name\":\"AU\",\"types\":[\"country\",\"political\"]},{\"long_name\":\"3380\",\"short_name\":\"3380\",\"types\":[\"postal_code\"]}],\"formatted_address\":\"Leviathan Rd, Stawell VIC 3380, Australia\",\"geometry\":{\"bounds\":{\"northeast\":{\"lat\":-37.0587865,\"lng\":142.8032973},\"southwest\":{\"lat\":-37.066546,\"lng\":142.8006253}},\"location\":{\"lat\":-37.0634265,\"lng\":142.8020888},\"location_type\":\"GEOMETRIC_CENTER\",\"viewport\":{\"northeast\":{\"lat\":-37.0587865,\"lng\":142.8033102802915},\"southwest\":{\"lat\":-37.066546,\"lng\":142.8006123197085}}},\"place_id\":\"EilMZXZpYXRoYW4gUmQsIFN0YXdlbGwgVklDIDMzODAsIEF1c3RyYWxpYQ\",\"types\":[\"route\"]}],\"status\":\"OK\"}")]
		[InlineData("{\"results\":[{\"address_components\":[{\"long_name\":\"Kurrajong Road\",\"short_name\":\"Kurrajong Rd\",\"types\":[\"route\"]},{\"long_name\":\"Wendouree\",\"short_name\":\"Wendouree\",\"types\":[\"locality\",\"political\"]},{\"long_name\":\"Ballarat City\",\"short_name\":\"Ballarat\",\"types\":[\"administrative_area_level_2\",\"political\"]},{\"long_name\":\"Victoria\",\"short_name\":\"VIC\",\"types\":[\"administrative_area_level_1\",\"political\"]},{\"long_name\":\"Australia\",\"short_name\":\"AU\",\"types\":[\"country\",\"political\"]},{\"long_name\":\"3355\",\"short_name\":\"3355\",\"types\":[\"postal_code\"]}],\"formatted_address\":\"Kurrajong Rd, Wendouree VIC 3355, Australia\",\"geometry\":{\"bounds\":{\"northeast\":{\"lat\":-37.52561590000001,\"lng\":143.8205315},\"southwest\":{\"lat\":-37.5263917,\"lng\":143.8176961}},\"location\":{\"lat\":-37.5258719,\"lng\":143.8191812},\"location_type\":\"GEOMETRIC_CENTER\",\"viewport\":{\"northeast\":{\"lat\":-37.5246548197085,\"lng\":143.8205315},\"southwest\":{\"lat\":-37.5273527802915,\"lng\":143.8176961}}},\"place_id\":\"ChIJPxbbiPRD0WoRGRjHhWAdixA\",\"types\":[\"route\"]}],\"status\":\"OK\"}")]
		[InlineData("{\"results\":[{\"address_components\":[{\"long_name\":\"24\",\"short_name\":\"24\",\"types\":[\"street_number\"]},{\"long_name\":\"Commercial Drive\",\"short_name\":\"Commercial Dr\",\"types\":[\"route\"]},{\"long_name\":\"Thomastown\",\"short_name\":\"Thomastown\",\"types\":[\"locality\",\"political\"]},{\"long_name\":\"Whittlesea City\",\"short_name\":\"Whittlesea\",\"types\":[\"administrative_area_level_2\",\"political\"]},{\"long_name\":\"Victoria\",\"short_name\":\"VIC\",\"types\":[\"administrative_area_level_1\",\"political\"]},{\"long_name\":\"Australia\",\"short_name\":\"AU\",\"types\":[\"country\",\"political\"]},{\"long_name\":\"3074\",\"short_name\":\"3074\",\"types\":[\"postal_code\"]}],\"formatted_address\":\"24 Commercial Dr, Thomastown VIC 3074, Australia\",\"geometry\":{\"location\":{\"lat\":-37.69312900000001,\"lng\":145.041586},\"location_type\":\"ROOFTOP\",\"viewport\":{\"northeast\":{\"lat\":-37.69178001970851,\"lng\":145.0429349802915},\"southwest\":{\"lat\":-37.69447798029151,\"lng\":145.0402370197085}}},\"place_id\":\"ChIJbcE3PUlP1moRupuraqyo2pg\",\"types\":[\"street_address\"]}],\"status\":\"OK\"}")]
		public void CanDecodeGoogleGeocodeResult(string jsonResult)
		{
			// Create the address parser and get the address object from it
			GoogleGeocodeWrapper wrapper = new GoogleGeocodeWrapper(new Mock<ILogger>().Object);
			StructuredAddress result = wrapper.GetStructuredAddressFromGoogleGeocode(jsonResult, "skdjfljkadflkajdfklajdflkdjf");

			// Ensure the result exist
			Assert.NotNull(result);

			// Ensure there is at least a suburb and state
			Assert.True(!String.IsNullOrEmpty(result.Suburb), "Suburb is empty or null.");
			Assert.True(!String.IsNullOrEmpty(result.State), "State is empty or null.");
		}

	}
}
