using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Events
{
	public enum EventJobStatus
	{

		[Description("Not started")]
		NotStarted = 1,

		[Description("In progress")]
		InProgress,

		Completed, 

		Cancelled, 

		[Description("Further information required")]
		FurtherInformationRequired

}
}
