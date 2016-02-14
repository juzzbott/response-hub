using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity;
using System.Security.Claims;

namespace Enivate.ResponseHub.Model.Identity
{
	public class IdentityUser : IEntity, IUser<Guid>
	{
		public Guid Id { get; set; }

		public string UserName { get; set; }

		public string PasswordHash { get; set; }

		public string EmailAddress { get; set; }

		public string FirstName { get; set; }

		public string Surname { get; set; }

		public DateTime Created { get; set; }

		public PasswordResetToken PasswordResetToken { get; set; }

		public IList<Claim> Claims { get; set; }

		public IList<UserLoginInfo> Logins { get; set; }

		public IdentityUser()
		{
			Id = Guid.NewGuid();
			Claims = new List<Claim>();
			Logins = new List<UserLoginInfo>();
		}

	}
}
