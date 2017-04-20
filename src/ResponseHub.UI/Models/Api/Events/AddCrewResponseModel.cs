using Enivate.ResponseHub.Model.Crews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Api.Events
{
	public class AddCrewResponseModel
	{

		public bool Success { get; set; }

		public string ErrorMessage { get; set; }

		public Crew Crew { get; set; }

	}
}
