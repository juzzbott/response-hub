using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Addresses.Interface;

namespace Enivate.ResponseHub.Model.Addresses.Mock
{
	public class MockAddressService : IAddressService
	{
		public Task<StructuredAddress> GetStructuredAddressByAddressQuery(string addressQuery)
		{
			return Task.FromResult<StructuredAddress>(null);
		}
	}
}
