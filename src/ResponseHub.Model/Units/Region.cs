using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Units
{
	public class Region : IEntity
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public string Capcode { get; set; }

		public ServiceType ServiceType { get; set; }

	}
}
