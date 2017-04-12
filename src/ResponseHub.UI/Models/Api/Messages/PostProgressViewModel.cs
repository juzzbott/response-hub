using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.UI.Models.Api.Messages
{
	public class PostProgressViewModel
	{

		public MessageProgressType ProgressType { get; set; }

		public string ProgressDateTime { get; set; }

	}
}