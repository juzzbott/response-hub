using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Groups.Interface
{
	public interface IGroupService
	{

		Task<Group> CreateGroup(string name, ServiceType service, string description);

	}

}
