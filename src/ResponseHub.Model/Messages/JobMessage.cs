using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Spatial;

namespace Enivate.ResponseHub.Model.Messages
{
	public class JobMessage : IEntity
	{

		public Guid Id { get; set; }

		public DateTime Timestamp { get; set; }

		public string Capcode { get; set; }

		public string MessageContent { get; set; }

		public string JobNumber { get; set; }

		public LocationInfo Location { get; set; }

		public MessagePriority Priority { get; set; }

		public IList<JobNote> Notes { get; set; }

		public JobMessage()
		{

			// Instantiate the id
			Id = Guid.NewGuid();

			// Create the location info so it's not null
			Location = new LocationInfo();

			// Default to administration.
			Priority = MessagePriority.Administration;

			Notes = new List<JobNote>();
		}

	}
}
