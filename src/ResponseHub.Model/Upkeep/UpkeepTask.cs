using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Upkeep
{
	public class UpkeepTask : IEntity
	{

		public Guid Id { get; set; }

		public Guid UnitId { get; set; }

		public string Name { get; set; }

		public Guid? AssetId { get; set; }

		public IList<string> TaskItems { get; set; }

		public UpkeepTask()
		{
			Id = Guid.NewGuid();
			TaskItems = new List<string>();
		}

	}
}
