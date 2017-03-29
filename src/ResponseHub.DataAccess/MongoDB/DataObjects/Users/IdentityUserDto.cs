using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Identity;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Users
{
	public class IdentityUserDto : IEntity
	{

		public Guid Id { get; set; }

		public string UserName { get; set; }

		public string PasswordHash { get; set; }

		public string EmailAddress { get; set; }

		public string FirstName { get; set; }

		public string Surname { get; set; }

		public DateTime Created { get; set; }

		public PasswordResetToken PasswordResetToken { get; set; }

		public IList<ClaimDto> Claims { get; set; }

		public IList<UserLoginInfo> Logins { get; set; }

		public string ActivationCode { get; set; }

		public UserProfile Profile { get; set; }

		public IdentityUserDto()
		{
			Id = Guid.NewGuid();
			Claims = new List<ClaimDto>();
			Logins = new List<UserLoginInfo>();
			Profile = new UserProfile();
		}

	}
}
