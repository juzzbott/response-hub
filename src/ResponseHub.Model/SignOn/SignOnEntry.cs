﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.SignOn
{
	public class SignOnEntry : IEntity
	{

		public Guid Id { get; set; }

		public Guid GroupId { get; set; }

		public Guid UserId { get; set; }

		public DateTime SignInTime { get; set; }

		public DateTime SignOutTime { get; set; }

		public SignOnType SignOnType { get; set; }

		public Activity ActivityDetails { get; set; }

		public SignOnEntry()
		{
			Id = Guid.NewGuid();
		}

	}
}
