using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.Model
{
	public interface IMailService
	{

		Task SendAccountActivationEmail(IdentityUser newUser);

		Task SendGroupCreatedEmail(IdentityUser groupAdmin, string groupName, ServiceType service, string capcode);

	}
}
