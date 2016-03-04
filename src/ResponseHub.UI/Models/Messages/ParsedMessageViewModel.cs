using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.UI.Models.Messages
{
	public class ParsedMessageViewModel
	{

		public Guid Id { get; set; }

		public string MessageBody { get; set; }

		public string MessageBodyShort { get; set; }

		public DateTime Timestamp { get; set; }

		public string JobNumber { get; set; }

		public string Capcode { get; set; }

		public string CapcodeGroupName { get; set; }

		public Guid CapcodeGroupId { get; set; }

		public MessagePriority Priority { get; set; }

		public LocationInfo Location { get; set; }


		public ParsedMessageViewModel()
		{
			Location = new LocationInfo();
		}

	}
}