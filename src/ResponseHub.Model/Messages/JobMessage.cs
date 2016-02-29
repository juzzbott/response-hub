using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Messages
{
	public class JobMessage : IEntity
	{

		public Guid Id { get; set; }

		public DateTime Timestamp { get; set; }

		public string Capcode { get; set; }

		public string MessageContent { get; set; }

		public string JobNumber { get; set; }

		public Location Location { get; set; }

		public MessagePriority Priority { get; set; }

		public JobMessage()
		{

			// Instantiate the id
			Id = Guid.NewGuid();

			Location = new Location();

			// Default to administration.
			Priority = MessagePriority.Administration;
		}

	}
}
