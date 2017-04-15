using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Units
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

		public bool IsUnitCapcode { get; set; }

		public string FormattedName()
		{
			return String.Format("{0} [{1}]", Name, ShortName);
		}

		public Capcode()
		{
			Id = Guid.NewGuid();
		}

	}
}
