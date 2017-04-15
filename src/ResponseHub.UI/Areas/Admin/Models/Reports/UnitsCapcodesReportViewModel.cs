using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Units;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Reports
{
	public class UnitsCapcodesReportViewModel
	{

		public string Id { get; set; }

		public string UnitName { get; set; }

		public string UnitCapcode { get; set; }

		public string Service { get; set; }

		public IList<Capcode> Capcodes { get; set; }

	}
}