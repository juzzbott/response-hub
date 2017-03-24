using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.SignIn;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.SignIn
{
	public class SignInEntryDto : IEntity
	{

		public Guid Id { get; set; }

		public DateTime Created { get; set; }

		public Guid GroupId { get; set; }

		public Guid UserId { get; set; }

		public DateTime SignInTime { get; set; }

		public DateTime SignOutTime { get; set; }

		public SignInType SignInType { get; set; }

		public ActivityDto ActivityDetails { get; set; }

		public SignInEntryDto()
		{
			Id = Guid.NewGuid();
			Created = DateTime.UtcNow;
		}

	}
}
