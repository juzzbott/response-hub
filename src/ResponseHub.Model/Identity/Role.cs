using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.Identity;

namespace Enivate.ResponseHub.Model.Identity
{
	public class Role : IEntity, IRole<Guid>
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

	}
}
