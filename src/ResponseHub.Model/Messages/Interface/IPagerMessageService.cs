using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Messages.Interface
{
	public interface IPagerMessageService
	{

		Task Save(PagerMessage pagerMessage);

	}
}
