using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.SignOn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.SignOn
{
	public class SignOnEntryDto : IEntity
	{

		public Guid Id { get; set; }

		public Guid GroupId { get; set; }

		public Guid UserId { get; set; }

		public DateTime SignInTime { get; set; }

		public DateTime SignOutTime { get; set; }

		public SignOnType SignOnType { get; set; }

		public ActivityDto ActivityDetails { get; set; }

		public SignOnEntryDto()
		{
			Id = Guid.NewGuid();
		}

	}
}
