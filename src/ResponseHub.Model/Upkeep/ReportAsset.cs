using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Upkeep
{
	public class ReportAsset
	{

		public Guid Id { get; set; }

		public Guid AssetId { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public ReportInventory Inventory { get; set; }

		public ReportAsset()
		{
			Id = Guid.NewGuid();
		}

	}
}
