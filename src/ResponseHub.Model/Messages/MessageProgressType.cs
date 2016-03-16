using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Messages
{
	public enum MessageProgressType
	{

		[Description("On route")]
		OnRoute = 1,

		[Description("On scene")]
		OnScene = 2,

		[Description("Job clear")]
		JobClear = 3

	}
}
