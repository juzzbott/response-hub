﻿using Enivate.ResponseHub.Model.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.UI.Models.Users;

namespace Enivate.ResponseHub.UI.Models.Events
{
	public class CrewViewModel
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public DateTime Created { get; set; }

		public DateTime Updated { get; set; }

		public UnitMemberViewModel CrewLeader { get; set; }

		public IList<UnitMemberViewModel> CrewMembers { get; set; }

		public IList<AssignedJobViewModel> AssignedJobs { get; set; }

		public CrewViewModel()
		{
			CrewMembers = new List<UnitMemberViewModel>();
			AssignedJobs = new List<AssignedJobViewModel>();
		}

	}
}