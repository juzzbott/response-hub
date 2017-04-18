using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Units.Interface
{
	public interface ICapcodeService
	{

		Task<IList<Capcode>> GetAll();

		Task<Capcode> Create(string name, string capcodeAddress, string shortName, ServiceType service, bool isUnitOnly);

		Task<IList<Capcode>> GetAllByService(ServiceType service);

		Task<IList<Capcode>> GetAllByService(ServiceType service, bool isUnitCapcode);

		Task<Capcode> GetById(Guid id);

		Task Save(Capcode capcode);

		Task<IList<Capcode>> FindByName(string name);

		Task<IList<Capcode>> GetCapcodesForUser(Guid userId);
		
		Task<IList<Capcode>> GetSharedCapcodes();
		
		Task<IList<Capcode>> GetAllByUnitOnly(bool unitOnly);

		Task<Capcode> GetByCapcodeAddress(string capcodeAddress);

		Task Remove(Guid id);

	}
}
