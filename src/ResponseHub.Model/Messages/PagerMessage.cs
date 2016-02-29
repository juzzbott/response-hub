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

		/// <summary>
		/// Returns the message in the original format from PDW.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("{0}  {1}  {2}  {3}  {4}  {5}  {6}",
				Address,
				Timestamp.ToString("HH:mm:ss"),
				Timestamp.ToString("yy-MM-dd"),
				Mode,
				Type,
				Bitrate,
				MessageContent);
		}

		public PagerMessage()
		{
			Id = Guid.NewGuid();
		}

	}
}
