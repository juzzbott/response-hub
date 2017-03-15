using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Decoding
{
	public class DecoderStatus
	{

		public DateTime LastCleanOperation { get; set; }

		public DateTime LastEmailWarning { get; set; }

		public IList<KeyValuePair<DateTime, string>> InvalidMessages { get; set; }

		/// <summary>
		/// Creates a new istance of the decoder status class, setting the last clean operation and last email warning datetimes to DateTime.MinValue.
		/// </summary>
		public DecoderStatus()
		{

			// Set the initial timestamps
			LastCleanOperation = DateTime.MinValue.ToUniversalTime();
			LastEmailWarning = DateTime.MinValue.ToUniversalTime();

			// Instantiate the list
			InvalidMessages = new List<KeyValuePair<DateTime, string>>();

		}
	}
}
