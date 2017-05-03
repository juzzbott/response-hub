using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.Api.SignIn
{
	public class PostSignInResponse
	{
		public DateTime SignInTime { get; set; }

		public string FullName { get; set; }

		public string MemberNumber { get; set; }
	}
}