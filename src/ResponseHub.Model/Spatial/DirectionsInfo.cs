using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Spatial
{
	public class DirectionsInfo : IEntity
	{

		public Guid Id { get; set; }

		public double TotalDistance { get; set; }

		public List<Coordinates> Coordinates { get; set; }

		public DirectionsInfo()
		{
			Coordinates = new List<Coordinates>();
		}

	}
}
