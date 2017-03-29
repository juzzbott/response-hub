using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Training
{
	public enum TrainingType
	{

		[Description("General")]
		General = 1,

		[Description("General rescue")]
		GeneralRescue = 2,

		[Description("Road rescue")]
		RoadRescue = 3,

		[Description("Land search")]
		LandSearch = 4,

		[Description("Rescue boat")]
		RescueBoat = 5,

		[Description("Chainsaws")]
		Chainsaws = 6,

		[Description("Storm & Water")]
		StormWater = 7,

		[Description("Communications")]
		Communications = 8,

		[Description("Other")]
		Other = 99

	}
}
