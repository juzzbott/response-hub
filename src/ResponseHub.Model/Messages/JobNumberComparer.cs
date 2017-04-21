using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Messages
{
	public class JobNumberComparer : IComparer<string>
	{
		
		public int Compare(string a, string b)
		{
			// if either of the job numbers is empty, then we can't compare, so just return -1
			if (String.IsNullOrEmpty(a) || String.IsNullOrEmpty(b))
			{
				return -1;
			}

			// String the F or S prefix
			int compareJobNumberA;
			Int32.TryParse(a.Substring(1), out compareJobNumberA);
			int compareJobNumberB;
			Int32.TryParse(b.Substring(1), out compareJobNumberB);

			// return the comparison results
			if (compareJobNumberA > compareJobNumberB)
			{
				return 1;
			}
			else if (compareJobNumberA < compareJobNumberB)
			{
				return -1;
			}
			else
			{
				return 0;
			}

		}

	}
}
