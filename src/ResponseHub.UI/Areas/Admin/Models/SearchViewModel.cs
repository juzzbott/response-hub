using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models
{
	public class SearchViewModel
	{

		public int Results { get; set; }

		public SearchViewModel()
		{

		}

		public SearchViewModel(int results)
		{
			Results = results;
		}

	}
}