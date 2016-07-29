using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Messages
{
	public enum MessagePriority
	{

		[Description("Emergency")]
		Emergency = 1,

		[Description("Non-emergency")]
		NonEmergency = 2,

		[Description("Administration")]
		Administration = 3

	}
}
