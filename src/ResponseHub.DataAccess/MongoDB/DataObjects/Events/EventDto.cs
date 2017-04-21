using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Crews;
using Enivate.ResponseHub.Model.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Events
{
	public class EventDto : IEntity
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public Guid UnitId { get; set; }

		public DateTime Created { get; set; }

		public DateTime EventStarted { get; set; }

		public DateTime? EventFinished { get; set; }

		public IList<Crew> Crews { get; set; }
		
		public EventDto()
		{
			Crews = new List<Crew>();
		}

	}
}
