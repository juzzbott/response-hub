using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Messages
{
	public class MessageProgress
	{

		public Guid UserId { get; set; }

		public MessageProgressType ProgressType { get; set; }

		public DateTime Timestamp { get; set; }

	}
}
