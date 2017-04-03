using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Groups
{
	public class CreateGroupModel
	{

		[Required(ErrorMessage = "You must enter a group name.")]
		public string Name { get; set; }
		
		[Required(ErrorMessage = "You must enter a capcode / pager address for this group.")]
		public string Capcode { get; set; }

		public string AdditionalCapcodes { get; set; }

		public IList<Capcode> AvailableAdditionalCapcodes { get; set; }

		public IList<Capcode> AvailableGroupCapcodes { get; set; }

		public string Description { get; set; }

		[Required(ErrorMessage = "You must enter a group administrator.")]
		[EmailAddress(ErrorMessage = "You must enter a valid email address.")]
		public string GroupAdministratorEmail { get; set; }

		public ConfirmUserViewModel GroupAdministrator { get; set; }

		[Required(ErrorMessage = "Please select a region for the group.")]
		public Guid Region { get; set; }

		public IList<SelectListItem> AvailableRegions { get; set; }

		/// <summary>
		/// Nullable type is used so that the value does not default to Zero on the form, as 0, 0 is a valid GPS coordinate. 
		/// Required field will ensure the user enters a value.
		/// </summary>
		[Required(ErrorMessage = "Please enter a latitude coordinate for the group HQ.")]
		[Range(-90, 90, ErrorMessage = "Please enter a latitude coordinate between -90 and 90.")]
		public double? Latitude { get; set; }

		/// <summary>
		/// Nullable type is used so that the value does not default to Zero on the form, as 0, 0 is a valid GPS coordinate. 
		/// Required field will ensure the user enters a value.
		/// </summary>
		[Required(ErrorMessage = "Please enter a longitude coordinate for the group HQ.")]
		[Range(-180, 180, ErrorMessage = "Please enter a longitude coordinate between -180 and 180.")]
		public double? Longitude { get; set; }

		public CreateGroupModel()
		{

			GroupAdministrator = new ConfirmUserViewModel();

			AvailableRegions = new List<SelectListItem>();

			AvailableAdditionalCapcodes = new List<Capcode>();

			AvailableGroupCapcodes = new List<Capcode>();

		}

	}
}