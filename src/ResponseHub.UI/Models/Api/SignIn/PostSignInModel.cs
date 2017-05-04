using Enivate.ResponseHub.Model.SignIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Api.SignIn
{
	public class PostSignInModel
	{

		public Guid JobMessageId { get; set; }

		public string Description { get; set; }

		public SignInType SignInType { get; set; }

	}
}