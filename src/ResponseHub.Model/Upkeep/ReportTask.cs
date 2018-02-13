using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Upkeep
{
	public class ReportTask
	{
		public Guid Id { get; set; }

		public Guid TaskId { get; set; }

		public string Name { get; set; }

		public IList<ReportItem> TaskItems { get; set; }

		public ReportAsset Asset { get; set; }

		public ReportTask()
		{
			Id = Guid.NewGuid();
			TaskItems = new List<ReportItem>();
		}
	}
}
