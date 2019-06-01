using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.Attachments;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.Model.Units;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.PagerMessages
{
	public class PagerMessageViewModel
	{

		public Guid Id { get; set; }

		public string MessageBody { get; set; }

		public IList<AdditionalMessage> AdditionalMessages { get; set; }

		public DateTime Timestamp { get; set; }

		public string JobNumber { get; set; }

		public string Capcode { get; set; }

		public string CapcodeUnitName { get; set; }

		public Guid CapcodeUnitId { get; set; }

		public MessagePriority Priority { get; set; }

		public LocationInfo Location { get; set; }

		public int Version { get; set; }

        public IList<Capcode> JobCapcodes { get; set; }

		public PagerMessageViewModel()
		{
			Location = new LocationInfo();
			AdditionalMessages = new List<AdditionalMessage>();
            JobCapcodes = new List<Capcode>();
		}
	}
}