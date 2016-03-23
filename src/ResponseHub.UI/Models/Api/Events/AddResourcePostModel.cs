using Enivate.ResponseHub.Model.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Api.Events
{
	public class AddResourcePostModel
	{

		public string Name { get; set; }

		public Guid AgencyId { get; set; }

		public Guid? UserId { get; set; }

		public ResourceType Type { get; set; }

	}
}