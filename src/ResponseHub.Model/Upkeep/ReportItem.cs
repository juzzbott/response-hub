using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Upkeep
{
	public class ReportItem
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public bool Checked { get; set; }

		public string Notes { get; set; }

		public int Quantity { get; set; }

		public ReportItem()
		{
			Id = Guid.NewGuid();
		}

	}
}
