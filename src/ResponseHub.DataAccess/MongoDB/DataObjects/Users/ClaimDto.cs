using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Users
{
	public class ClaimDto
	{

		public string Type { get; set; }

		public string Value { get; set; }

		public string Issuer { get; set; }

	}
}
