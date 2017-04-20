using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Api.Events
{
	public class AssignJobsToCrewPostModel
	{

		public Guid CrewId { get; set; }

		public IList<Guid> JobMessageIds { get; set; }

		public AssignJobsToCrewPostModel()
		{
			JobMessageIds = new List<Guid>();
		}

	}
}