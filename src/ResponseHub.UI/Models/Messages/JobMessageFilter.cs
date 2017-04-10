using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Messages
{
	public class JobMessageFilter
	{

		public DateTime? DateFrom { get; set; }

		public DateTime? DateTo { get; set; }
		
		public bool FilterApplied { get; set; }

	}
}