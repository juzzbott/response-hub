using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.SignOn
{
	public class OperationActivity : Activity
	{

		public Guid JobId { get; set; }

		public string Description { get; set; }

	}
}
