using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Upkeep
{
	public class UpkeepReport : IEntity
	{

		public Guid Id { get; set; }

		public Guid UnitId { get; set; }

		public string Name { get; set; }

		public DateTime Created { get; set; }

		public Guid CreatedBy { get; set; }

		public DateTime? Submitted { get; set; }

		public Guid? SubmittedBy { get; set; }

		public IList<ReportTask> Tasks { get; set; }

		public UpkeepReport()
		{
			Id = Guid.NewGuid();
			Tasks = new List<ReportTask>();
		}


	}
}
