using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.SignIn
{
	public enum OtherSignInType
	{

		Meeting = 1,

		Maintenance = 2,

		[Description("Finance & administration")]
		FinanceAdministration = 3,

		[Description("Community event")]
		CommunityEvent = 4,

		[Description("Driver reviver")]
		DriverReviver = 5, 

		Other = 99

	}
}
