using System;
using System.Collections.Generic;

using Enivate.ResponseHub.UI.Models.Users;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Groups
{
	public class SingleGroupViewModel
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public string Service { get; set; }

		public string Description { get; set; }

		public string Capcode { get; set; }

		public IList<GroupUserViewModel> Users { get; set; }

		public SingleGroupViewModel()
		{
			Users = new List<GroupUserViewModel>();
		}

	}
}