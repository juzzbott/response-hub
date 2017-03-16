using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model
{
	public class PagedResultSet<T>
	{

		public int Limit { get; set; }

		public int Skip { get; set; }

		public int TotalResults { get; set; }

		public IList<T> Items { get; set; }

		public static PagedResultSet<T> Empty
		{
			get
			{
				return new PagedResultSet<T>();
			}
		}

		/// <summary>
		/// Creates a new instance of the search result set.
		/// </summary>
		public PagedResultSet()
		{
			Items = new List<T>();
		}

	}
}
