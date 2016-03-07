using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Groups
{
	public class Capcode : IEntity
	{

		public Guid Id { get; set; }

		public string CapcodeAddress { get; set; }

		public string Name { get; set; }

		public string ShortName { get; set; }

		public ServiceType Service { get; set; }

		public DateTime Created { get; set; }

		public DateTime Updated { get; set; }

		public Capcode()
		{
			Id = Guid.NewGuid();
		}

	}
}
