using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Units
{
	public class CreateUnitModel
	{

		[Required(ErrorMessage = "You must enter a unit name.")]
		public string Name { get; set; }
		
		[Required(ErrorMessage = "You must enter a capcode / pager address for this unit.")]
		public string Capcode { get; set; }

		public string AdditionalCapcodes { get; set; }

		public IList<Capcode> AvailableAdditionalCapcodes { get; set; }

		public IList<Capcode> AvailableUnitCapcodes { get; set; }

		public string Description { get; set; }

		[Required(ErrorMessage = "You must enter a unit administrator.")]
		[EmailAddress(ErrorMessage = "You must enter a valid email address.")]
		public string UnitAdministratorEmail { get; set; }

		public ConfirmUserViewModel UnitAdministrator { get; set; }

		[Required(ErrorMessage = "Please select a region for the unit.")]
		public Guid Region { get; set; }

		public IList<SelectListItem> AvailableRegions { get; set; }

		/// <summary>
		/// Nullable type is used so that the value does not default to Zero on the form, as 0, 0 is a valid GPS coordinate. 
		/// Required field will ensure the user enters a value.
		/// </summary>
		[Required(ErrorMessage = "Please enter a latitude coordinate for the unit HQ.")]
		[Range(-90, 90, ErrorMessage = "Please enter a latitude coordinate between -90 and 90.")]
		public double? Latitude { get; set; }

		/// <summary>
		/// Nullable type is used so that the value does not default to Zero on the form, as 0, 0 is a valid GPS coordinate. 
		/// Required field will ensure the user enters a value.
		/// </summary>
		[Required(ErrorMessage = "Please enter a longitude coordinate for the unit HQ.")]
		[Range(-180, 180, ErrorMessage = "Please enter a longitude coordinate between -180 and 180.")]
		public double? Longitude { get; set; }

		public CreateUnitModel()
		{

			UnitAdministrator = new ConfirmUserViewModel();

			AvailableRegions = new List<SelectListItem>();

			AvailableAdditionalCapcodes = new List<Capcode>();

			AvailableUnitCapcodes = new List<Capcode>();

		}

	}
}