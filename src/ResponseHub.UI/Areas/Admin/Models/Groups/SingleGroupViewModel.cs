using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Groups
{
	public class SingleGroupViewModel
	{

		public string Name { get; set; }

		public ServiceType Service { get; set; }

		public string Description { get; set; }

	}
}