using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.UI.Models.Api.Search
{
	public class SearchPostModel
	{

		public string Keywords { get; set; }

		public int Skip { get; set; }

		public IList<MessageType> MessageTypes { get; set; }

		public string DateFrom { get; set; }

		public string DateTo { get; set; }

		public SearchPostModel()
		{
			MessageTypes = new List<MessageType>();
		}

	}
}