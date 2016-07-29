using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model;

namespace Enivate.ResponseHub.Tests.Unit
{
	public class FakeEntity : IEntity
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public string OtherData { get; set; }

	}
}
