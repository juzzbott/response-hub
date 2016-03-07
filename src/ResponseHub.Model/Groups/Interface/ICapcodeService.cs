using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Groups.Interface
{
	public interface ICapcodeService
	{

		Task<IList<Capcode>> GetAll();

		Task<Capcode> Create(string name, string capcodeAddress, string shortName, ServiceType service);

		Task<IList<Capcode>> GetAllByService(ServiceType service);

		Task<Capcode> GetById(Guid id);

		Task Save(Capcode capcode);

	}
}
