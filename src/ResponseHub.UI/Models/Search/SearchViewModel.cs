using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.UI.Models.Messages;

namespace Enivate.ResponseHub.UI.Models.Search
{
	public class SearchViewModel
	{

		public string SearchKeywords { get; set; }

		public int TotalResults { get; set; }

		public JobMessageListViewModel Results { get; set; }

		public DateTime? DateFrom { get; set; }

		public DateTime? DateTo { get; set; }

		public MessageType MessageTypes { get; set; }

		public SearchViewModel()
		{
		}

	}
}