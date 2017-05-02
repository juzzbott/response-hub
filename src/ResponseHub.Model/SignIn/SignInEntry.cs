using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.SignIn
{
	public class SignInEntry : IEntity
	{

		public Guid Id { get; set; }

		public DateTime Created { get; set; }

		public Guid UnitId { get; set; }

		public Guid UserId { get; set; }

		public DateTime SignInTime { get; set; }

		public DateTime? SignOutTime { get; set; }

		public SignInType SignInType { get; set; }

		public OperationActivity OperationDetails { get; set; }

		public TrainingActivity TrainingDetails { get; set; }

		public OtherActivity OtherDetails { get; set; }

		public SignInEntry()
		{
			Id = Guid.NewGuid();
			Created = DateTime.UtcNow;
		}

	}
}
