﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Events.Interface
{
	public interface IEventService
	{

		Task<Event> CreateEvent(string name, Guid groupId, Guid userId, DateTime startDate);

		Task<Event> GetById(Guid id);

		Task<IList<Event>> GetEventsByGroup(IEnumerable<Guid> groupIds);

		Task<IList<Event>> FindByKeywords(string keywords, IEnumerable<Guid> groupIds);

	}
}