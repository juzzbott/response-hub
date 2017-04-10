using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Groups;

namespace Enivate.ResponseHub.UI.Models.Messages
{
	public class JobMessageListViewModel
	{

		public IList<JobMessageViewModel> Messages { get; set; }

		public IList<Capcode> UserCapcodes { get; set; }

		public JobMessageFilter Filter { get; set; }

		public MessageType MessageType { get; set; }

		public JobMessageListViewModel()
		{
			Messages = new List<JobMessageViewModel>();
			UserCapcodes = new List<Capcode>();
			Filter = new JobMessageFilter();
		}

	}
}