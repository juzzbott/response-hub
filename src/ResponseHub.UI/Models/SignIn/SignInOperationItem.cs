using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.SignIn
{
	public class SignInOperationItem
	{

		public Guid JobId { get; set; }

		public string JobNumber { get; set; }

		public string Description { get; set; }

		public DateTime Timestamp { get; set; }

	}
}