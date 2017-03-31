using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Training.Interface
{
	public interface ITrainingSessionService
	{

		Task CreateTrainingSession(TrainingSession trainingSession);

		Task<IList<TrainingSession>> GetTrainingSessionsForGroup(Guid groupId);

		Task<IList<TrainingSession>> GetTrainingSessionsForGroup(Guid groupId, int limit);

		Task<TrainingSession> GetById(Guid id);

		Task SaveTrainingSession(TrainingSession trainingSession);

	}
}
