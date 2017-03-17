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

		public IList<JobMessageViewModel> Messages { get; set; }

		public WallboardViewModel()
		{
			Messages = new List<JobMessageViewModel>();
		}

	}
}