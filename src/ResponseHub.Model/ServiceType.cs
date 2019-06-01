using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model
{
	public enum ServiceType
	{

		[Description("All Services")]
		AllServices = 1,

        [Description("Unknown")]
        Unknown = 2,

		[Description("State Emergency Service")]
		StateEmergencyService = 5,

		[Description("Country Fire Authority")]
		CountryFireAuthority = 6

	}
}
