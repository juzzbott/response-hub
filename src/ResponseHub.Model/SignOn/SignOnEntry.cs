using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.SignOn
{
	public class SignOnEntry : IEntity
	{

		public Guid Id { get; set; }

		public Guid GroupId { get; set; }

		public Guid UserId { get; set; }

		public DateTime SignInTime { get; set; }

		public SignOnType SignOnType { get; set; }

		public string OperationDescription { get; set; }

		public Guid? OperationJobId { get; set; }

		public TrainingType TrainingType { get; set; }

		public string TrainingTypeOther { get; set; }

	}
}
