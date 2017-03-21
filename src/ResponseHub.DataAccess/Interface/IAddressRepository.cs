using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Addresses;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Addresses;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface IAddressRepository
	{

		Task<StructuredAddress> GetByAddressQueryHash(string addressQueryHash);

		Task<StructuredAddress> GetById(Guid id);

		Task<StructuredAddress> Add(StructuredAddress address);

	}
}
