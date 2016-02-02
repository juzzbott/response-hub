using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity;

using Enivate.ResponseHub.Model.Identity;
using Enivate.ResponseHub.Model.Identity.Interface;

namespace Enivate.ResponseHub.ApplicationServices.Identity
{
	public class UserService : UserManager<User, Guid>
	{

		IUserRepository _repository;

		/// <summary>
		/// Creates a new instance of the UserService method.
		/// </summary>
		/// <param name="repository"></param>
		public UserService(IUserRepository repository) : base((IUserStore<User, Guid>)repository)
		{
			_repository = repository;
		}
	}
}
