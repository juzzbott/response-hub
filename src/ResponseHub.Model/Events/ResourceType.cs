﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Events
{
	public enum ResourceType
	{

		[Description("Group member")]
		GroupMember = 1, 

		[Description("Additional resource")]
		AdditionalResource = 2

	}
}
