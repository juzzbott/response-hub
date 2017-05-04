using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.UI.Models.Messages
{
	public class JobMessageSignInViewModel
	{

		public Guid JobId { get; set; }

		public string JobNumber { get; set; }

		public MessagePriority Priority { get; set; }

		public IDictionary<Guid, string> AvailableMembers { get; set; }

		public DateTime Timestamp { get; set; }

		public string CapcodeUnitName { get; set; }

		public string SelectedMembers { get; set; }

		public JobMessageSignInViewModel()
		{
			AvailableMembers = new Dictionary<Guid, string>();
		}

	}
}