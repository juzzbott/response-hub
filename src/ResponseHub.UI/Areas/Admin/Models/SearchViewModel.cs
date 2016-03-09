using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models
{
	public class SearchViewModel
	{

		public int Results { get; set; }

		public string SearchLabel { get; set; }

		public SearchViewModel()
		{

		}

		public SearchViewModel(int results, string searchLabel)
		{
			Results = results;
			SearchLabel = searchLabel;
		}

	}
}