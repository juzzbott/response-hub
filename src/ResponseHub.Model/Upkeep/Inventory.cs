using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Upkeep
{
	public class Inventory
	{

		public IList<Container> Containers { get; set; }

		public IList<CatalogItem> Items { get; set; }

		public Inventory()
		{
			Containers = new List<Container>();
			Items = new List<CatalogItem>();
		}
	}
}
