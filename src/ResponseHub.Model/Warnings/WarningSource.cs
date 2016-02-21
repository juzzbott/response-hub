using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Warnings
{
	public enum WarningSource
	{

		[Description("Bureau of Meteorology")]
		BureauOfMeteorology = 1,

		[Description("State Emergency Service")]
		StateEmergencyService = 2,

		[Description("Country Fire Authority")]
		CountryFireAuthority = 4

}
}
