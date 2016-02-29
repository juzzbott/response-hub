using Enivate.ResponseHub.Mail;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Enivate.ResponseHub.Tests.Unit
{
	public class MailProviderTests
	{

		[Theory(DisplayName = "Can parse mail address - From raw format.")]
		[InlineData("Test User {test@domain.com}", "Test User", "test@domain.com")]
		[InlineData("Test{test@domain.com}", "Test", "test@domain.com")]
		[InlineData("test@domain.com", "", "test@domain.com")]
		[Trait("Category", "MailProvider Tests")]
		public void CanParseAddress_FromRawFormat(string rawMailAddress, string actualName, string actualEmail)
		{

			// Bamboo can't handle [] when parsing the unit tests, so we need to replace them.
			rawMailAddress = rawMailAddress.Replace('{', '[').Replace('}', ']');

			// Get the mail address object
			MailProvider provider = new MailProvider();
			MailAddress address = provider.GetMailAddress(rawMailAddress);

			// Asser the mail address is not null, and the actual values match
			Assert.NotNull(address);
			Assert.True(address.Address.Equals(actualEmail, StringComparison.CurrentCultureIgnoreCase));
			Assert.True(address.DisplayName.Equals(actualName, StringComparison.CurrentCultureIgnoreCase));

		}

		[Theory(DisplayName = "Can parse multiple mail addresses - From raw format.")]
		[InlineData("Test User {test@domain.com}, Test User {test@domain.com}, Test User {test@domain.com}", 3)]
		[InlineData("Test{test@domain.com}, Test{test@domain.com}", 2)]
		[InlineData("Testy{test@domain.com}", 1)]
		[Trait("Category", "MailProvider Tests")]
		public void CanParseMultipleAddresses_FromRawFormat(string rawAddresses, int count)
		{

			// Bamboo can't handle [] when parsing the unit tests, so we need to replace them.
			rawAddresses = rawAddresses.Replace('{', '[').Replace('}', ']');

			// Get the mail address object
			MailProvider provider = new MailProvider();
			MailAddressCollection addresses = provider.GetMailAddressCollection(rawAddresses);

			// Validate the tests
			Assert.NotNull(addresses);
			Assert.True(addresses.Count == count);
			foreach(MailAddress address in addresses)
			{
				Assert.False(String.IsNullOrEmpty(address.Address));
				Assert.False(String.IsNullOrEmpty(address.DisplayName));
			}
		}

	}
}
