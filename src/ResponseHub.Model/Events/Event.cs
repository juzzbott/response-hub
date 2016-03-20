using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Events
{
	public class Event : IEntity
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public Guid GroupId { get; set; }

		public DateTime Created { get; set; }

		public DateTime EventStarted { get; set; }

		public DateTime EventFinished { get; set; }

		public Event()
		{
			Id = Guid.NewGuid();
		}

	}
}
