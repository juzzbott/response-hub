using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Addresses.Interface
{
	public interface IAddressService
	{

		Task<StructuredAddress> GetStructuredAddressByAddressQuery(string addressQuery);

	}
}
