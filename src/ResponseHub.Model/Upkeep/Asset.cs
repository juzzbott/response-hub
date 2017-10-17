using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Upkeep
{
	public class Asset : IEntity
	{

		public Guid Id { get; set; }

		public Guid UnitId { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public Inventory Inventory { get; set; }

		public Asset()
		{
			Id = Guid.NewGuid();
		}

	}
}
