using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Training;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Training
{
	public class TrainingSessionDto : IEntity
	{

		public Guid Id { get; set; }

		public DateTime Created { get; set; }

		public Guid GroupId { get; set; }

		public DateTime SessionDate { get; set; }

		public IList<Guid> TrainingTypeIds { get; set; }

		public IList<Guid> Members { get; set; }

		public IList<Guid> Trainers { get; set; }

		public string Description { get; set; }

		public TrainingSessionType SessionType { get; set; }

		public decimal Duration { get; set; }

		public TrainingSessionDto()
		{
			Id = Guid.NewGuid();
			Members = new List<Guid>();
			Trainers = new List<Guid>();
			TrainingTypeIds = new List<Guid>();
		}

	}
}
