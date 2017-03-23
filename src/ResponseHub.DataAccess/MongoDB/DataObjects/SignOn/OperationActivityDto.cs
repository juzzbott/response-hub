using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.SignOn
{
	public class OperationActivityDto : ActivityDto
	{

		public Guid JobId { get; set; }

		public string Description { get; set; }

	}
}
