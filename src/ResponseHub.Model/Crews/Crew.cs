using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Crews
{
	public class Crew : IEntity
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public IList<Guid> JobMessageIds { get; set; }

		public IList<Guid> CrewMembers { get; set; }

		public Crew()
		{
			Id = Guid.NewGuid();
			JobMessageIds = new List<Guid>();
			CrewMembers = new List<Guid>();
		}

	}
}
