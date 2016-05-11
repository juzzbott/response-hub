using Enivate.ResponseHub.Model.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Api.Messages
{
	public class PostGetLatestFromLastModel
	{

		public Guid LastMessageId { get; set; }

		public MessageType MessageType { get; set; }

	}
}