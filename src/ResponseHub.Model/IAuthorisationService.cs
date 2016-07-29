using Enivate.ResponseHub.Model.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model
{
	public interface IAuthorisationService
	{

		Task<bool> CanEditEvent(Event eventObj, Guid userId);

	}
}
