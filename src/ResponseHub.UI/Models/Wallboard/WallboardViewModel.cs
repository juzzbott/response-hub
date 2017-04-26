using Enivate.ResponseHub.Model.Warnings;
using Enivate.ResponseHub.UI.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Wallboard
{
	public class WallboardViewModel
	{

		public IList<JobMessageListItemViewModel> JobMessages { get; set; }

		public WallboardViewModel()
		{
			JobMessages = new List<JobMessageListItemViewModel>();
		}

	}
}