using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Groups;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Reports
{
	public class GroupsCapcodesReportViewModel
	{

		public string Id { get; set; }

		public string GroupName { get; set; }

		public string GroupCapcode { get; set; }

		public string Service { get; set; }

		public IList<Capcode> Capcodes { get; set; }

	}
}