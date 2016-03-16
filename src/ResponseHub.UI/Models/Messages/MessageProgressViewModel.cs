using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Messages
{
	public class MessageProgressViewModel
	{

		public DateTime Timestamp { get; set; }

		public Guid UserId { get; set; }

		public string UserFullName { get; set; }

	}
}