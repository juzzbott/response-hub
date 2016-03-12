using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Messages
{
	public class JobNote : IEntity
	{

		public Guid Id { get; set; }

		public Guid UserId { get; set; }

		public DateTime Created { get; set; }

		public string Body { get; set; }

		public bool IsWordBack { get; set; }

		public JobNote()
		{
			Id = Guid.NewGuid();
		}

	}
}
