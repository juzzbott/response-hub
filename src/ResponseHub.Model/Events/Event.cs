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

		public string Description { get; set; }

		public Guid UnitId { get; set; }

		public DateTime Created { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime? FinishedDate { get; set; }

		public IList<Crew> Crews { get; set; }

		public IList<Guid> JobMessageIds { get; set; }

		public Event()
		{
			Id = Guid.NewGuid();
			Crews = new List<Crew>();
			JobMessageIds = new List<Guid>();
		}

	}
}
