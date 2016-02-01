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

		[Description("State Emergency Service")]
		StateEmergencyService = 1,

		[Description("Country Fire Authority")]
		CountryFireAuthority = 2

	}
}
