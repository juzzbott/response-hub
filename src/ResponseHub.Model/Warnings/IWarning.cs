using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Warnings
{
	public interface IWarning
	{

		DateTime Timestamp { get; set; }

		string ShortDescription { get; set; }

		WarningSource Source { get; set; }

		string Link { get; set; }

	}
}
