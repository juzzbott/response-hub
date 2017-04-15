using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.SignIn;

namespace Enivate.ResponseHub.UI.Models.SignIn
{
	public class SignInEntryListItemViewModel
	{

		public Guid Id { get; set; }

		public Guid UnitId { get; set; }

		public string UnitName { get; set; }

		public string UserId { get; set; }

		public string UserFullName { get; set; }

		public DateTime SignInTime { get; set; }

		public DateTime? SignOutTime { get; set; }

		public string SignInType { get; set; }

		public string Description { get; set; }

	}
}