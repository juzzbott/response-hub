using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Units;

namespace Enivate.ResponseHub.UI.Models.Messages
{
	public class JobMessageListViewModel
	{

		public IList<JobMessageListItemViewModel> JobMessages { get; set; }

		public JobMessageFilter Filter { get; set; }

		public MessageType MessageType { get; set; }

		public JobMessageListViewModel()
		{
			JobMessages = new List<JobMessageListItemViewModel>();
			Filter = new JobMessageFilter();
		}

	}
}