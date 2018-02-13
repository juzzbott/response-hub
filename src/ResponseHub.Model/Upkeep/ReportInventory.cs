using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Upkeep
{
	public class ReportInventory
	{

		public IList<ReportContainer> Containers { get; set; }

		public IList<ReportItem> Items { get; set; }

		public ReportInventory()
		{
			Containers = new List<ReportContainer>();
			Items = new List<ReportItem>();
		}

	}
}
