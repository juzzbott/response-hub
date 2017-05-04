using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Training;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Training;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface ITrainingSessionRepository
	{

		Task<IList<TrainingSession>> GetTrainingSessionsForUnit(Guid unitId, IEnumerable<TrainingType> trainingTypes);

		Task<IList<TrainingSession>> GetTrainingSessionsForUnit(Guid unitId, DateTime from, DateTime to, IEnumerable<TrainingType> trainingTypes);

		Task<IList<TrainingSession>> GetTrainingSessionsForUnit(Guid unitId, int limit, IEnumerable<TrainingType> trainingTypes);

		Task Add(TrainingSession session);

		Task<TrainingSession> GetById(Guid id, IEnumerable<TrainingType> trainingTypes);

		Task Save(TrainingSession session);

	}
}
