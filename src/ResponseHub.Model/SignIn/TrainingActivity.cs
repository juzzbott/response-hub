using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Training;

namespace Enivate.ResponseHub.Model.SignIn
{
	public class TrainingActivity : Activity
	{

		public TrainingType TrainingType { get; set; }

		public string OtherDescription { get; set; }

	}
}
