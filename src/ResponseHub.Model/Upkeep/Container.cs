using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Upkeep
{
	public class Container
	{

		public string Name { get; set; }

		public IList<Container> Containers { get; set; }

		public IList<CatalogItem> Items { get; set; }

		public Container()
		{
			Containers = new List<Container>();
			Items = new List<CatalogItem>();
		}

	}
}
