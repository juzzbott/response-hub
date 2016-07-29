using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.UI.Models.Messages;

namespace Enivate.ResponseHub.UI.Models.Api.Messages
{
	public class MessageProgressResponseModel : MessageProgressViewModel
	{

		public bool Success { get; set; }

		public string ErrorMessage { get; set; }

	}
}