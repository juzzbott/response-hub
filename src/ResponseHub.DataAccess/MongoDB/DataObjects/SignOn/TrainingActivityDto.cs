using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.SignOn;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.SignOn
{
	public class TrainingActivityDto : ActivityDto
	{

		public TrainingType TrainingType { get; set; }

		public string OtherDescription { get; set; }

	}
}
