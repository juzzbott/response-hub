using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.UI.Models.Messages
{
	public class ParsedMessageViewModel
	{

		public string MessageBody { get; set; }

		public DateTime Timestamp { get; set; }

		public string JobNumber { get; set; }

		public string Capcode { get; set; }

		public string CapcodeGroupName { get; set; }

		public Guid CapcodeGroupId { get; set; }

		public MessagePriority Priority { get; set; }

	}
}