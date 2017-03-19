using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Addresses;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface IAddressRepository : IRepository<StructuredAddress>
	{

		Task<StructuredAddress> GetByAddressQueryHash(string addressQueryHash);

	}
}
