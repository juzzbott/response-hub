using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.SignIn
{
	[Flags]
	public enum SignInType
	{

		Operation = 1,

		Training = 2,

		Other = 4

	}
}
