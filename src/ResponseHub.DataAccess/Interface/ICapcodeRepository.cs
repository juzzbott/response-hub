﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Groups;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface ICapcodeRepository : IRepository<Capcode>
	{

		Task<IList<Capcode>> FindByName(string name);

		Task<IList<Capcode>> GetCapcodesById(IList<Guid> capcodeIds);

	}
}