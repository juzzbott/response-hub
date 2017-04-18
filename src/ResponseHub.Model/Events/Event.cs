using Enivate.ResponseHub.Model.Crews;
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

		public Guid UnitId { get; set; }

		public DateTime Created { get; set; }

		public DateTime EventStarted { get; set; }

		public DateTime EventFinished { get; set; }

		public IList<EventResource> Resources { get; set; }

		public IList<Crew> Crews { get; set; }

		public Event()
		{
			Id = Guid.NewGuid();
			Resources = new List<EventResource>();
			Crews = new List<Crew>();
		}

	}
}
