using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Identity;

namespace Enivate.ResponseHub.Model.Units
{
	public class UserMapping
	{

		public Guid UserId { get; set; }

		public string Role { get; set; }

		public UserMapping()
		{
			// By default, set role to General User
			Role = RoleTypes.GeneralUser;
		}

	}
}
