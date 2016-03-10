using Enivate.ResponseHub.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common.Extensions;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Capcodes
{
	public class CreateCapcodeViewModel
	{

		public Guid Id { get; set; }

		[Required(ErrorMessage = "Please enter a friendly name for the capcode")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Please enter the short name for the capcode")]
		public string ShortName { get; set; }

		[Required(ErrorMessage = "Please enter a capcode address")]
		[Display(Name = "Capcode address")]
		public string CapcodeAddress { get; set; }

		[Required(ErrorMessage = "You must select a service the capcode is associated with", AllowEmptyStrings = false)]
		public string Service { get; set; }

		public IList<SelectListItem> AvailableServices { get; set; }

		public CreateCapcodeViewModel()
		{

			AvailableServices = new List<SelectListItem>();
			AvailableServices.Add(new SelectListItem() { Value = "", Text = "Please select" });

			foreach(ServiceType service in Enum.GetValues(typeof(ServiceType)).Cast<ServiceType>())
			{
				AvailableServices.Add(new SelectListItem() { Value = ((int)service).ToString(), Text = service.GetEnumDescription() });
			}
		}

	}
}