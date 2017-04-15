using Enivate.ResponseHub.Model.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Units
{
	public class Unit : IEntity
	{

		public Guid Id { get; set; }

		public string Capcode { get; set; }

		public IList<Guid> AdditionalCapcodes { get; set; }

		public string Name { get; set; }

		public DateTime Created { get; set; }

		public DateTime Updated { get; set; }

		public ServiceType Service { get; set; }

		public string Description { get; set; }

		public IList<UserMapping> Users { get; set; }

		public Region Region { get; set; }

		public Coordinates HeadquartersCoordinates { get; set; }

		public Unit()
		{
			Id = Guid.NewGuid();
			Users = new List<UserMapping>();
			AdditionalCapcodes = new List<Guid>();
		}

	}
}
