using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Agencies
{
	public class Agency : IEntity
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

	}
}
