using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Api.Events
{
	public class AddResourceResponseModel
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public Guid AgencyId { get; set; }

		public string AgencyName { get; set; }

	}
}