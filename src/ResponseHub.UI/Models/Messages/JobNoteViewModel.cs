using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Messages
{
	public class JobNoteViewModel
	{

		public Guid Id { get; set; }

		public Guid UserId { get; set; }

		public string UserDisplayName { get; set; }

		public DateTime Created { get; set; }

		public string Body { get; set; }

		public bool IsWordBack { get; set; }

	}
}