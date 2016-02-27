using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Messages
{
	public class PagerMessage : IEntity
	{

		public Guid Id { get; set; }

		public string Address { get; set; }

		public DateTime Timestamp { get; set; }

		public string Mode { get; set; }

		public string Type { get; set; }

		public int Bitrate { get; set; }

		public string MessageContent { get; set; }

		public string ShaHash { get; set; }

		public PagerMessage()
		{
			Id = Guid.NewGuid();
		}

	}
}
