using System;
using System.Collections.Generic;

using Enivate.ResponseHub.UI.Models.Users;
using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.Model.Units;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Units
{
	public class SingleUnitViewModel
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public string Service { get; set; }

		public string Region { get; set; }

		public string Description { get; set; }

		public string Capcode { get; set; }

		public IList<UnitUserViewModel> Users { get; set; }

		public Coordinates HeadquartersCoordinates { get; set; }

		public IList<Capcode> AdditionalCapcodes { get; set; }

		public SingleUnitViewModel()
		{
			Users = new List<UnitUserViewModel>();
			AdditionalCapcodes = new List<Capcode>();
		}

	}
}