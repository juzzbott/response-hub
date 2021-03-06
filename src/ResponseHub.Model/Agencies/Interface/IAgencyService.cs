using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Agencies.Interface
{
	public interface IAgencyService
	{

		Task<IList<Agency>> GetAll();

		Task<Agency> GetByID(Guid id);

	}
}
