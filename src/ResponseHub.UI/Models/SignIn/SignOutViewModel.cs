using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Models.SignIn
{
	public class SignOutViewModel
	{

		[Required(ErrorMessage = "Please ensure you have selected a date.")]
		public string SignOutDate { get; set; }

		[Required(ErrorMessage = "Please ensure a selected a time.")]
		public string SignOutTime { get; set; }

		public IList<SignInEntryListItemViewModel> SignIns { get; set; }

		public SignOutViewModel()
		{
			SignIns = new List<SignInEntryListItemViewModel>();
		}

	}
}