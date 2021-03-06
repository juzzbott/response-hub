using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Training
{
	public class TrainingType : IEntity
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public string ShortName { get; set; }

		public string Description { get; set; }

		public short SortOrder { get; set; }

		public TrainingType()
		{
			Id = Guid.NewGuid();
		}

	}
}
