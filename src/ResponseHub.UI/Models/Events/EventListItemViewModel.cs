using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Events
{
	public class EventListItemViewModel
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public Guid UnitId { get; set; }

		public string UnitName { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime? FinishDate { get; set; }

		public int JobsCount { get; set; }

	}
}