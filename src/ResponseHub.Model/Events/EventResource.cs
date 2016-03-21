using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Events
{
	public class EventResource
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public Guid? UserId { get; set; }

		public bool Active { get; set; }

		public ResourceType Type { get; set; }

		public Guid AgencyId { get; set; }

	}
}
