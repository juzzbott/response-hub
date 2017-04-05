using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Training
{
	public class TrainingSession : IEntity
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public DateTime Created { get; set; }

		public Guid GroupId { get; set; }

		public DateTime SessionDate { get; set; }

		public IList<TrainingType> TrainingTypes { get; set; }

		public IList<Guid> Members { get; set; }

		public IList<Guid> Trainers { get; set; }

		public string Description { get; set; }

		public TrainingSessionType SessionType { get; set; }

		public decimal Duration { get; set; }

		public TrainingSession()
		{
			Id = Guid.NewGuid();
			Members = new List<Guid>();
			Trainers = new List<Guid>();
			TrainingTypes = new List<TrainingType>();
		}

	}
}
