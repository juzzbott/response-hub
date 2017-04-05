using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Training.Interface
{
	public interface ITrainingService
	{

		Task CreateTrainingSession(TrainingSession trainingSession);

		Task<IList<TrainingSession>> GetTrainingSessionsForGroup(Guid groupId);

		Task<IList<TrainingSession>> GetTrainingSessionsForGroup(Guid groupId, int limit);

		Task<TrainingSession> GetTrainingSessionById(Guid id);

		Task SaveTrainingSession(TrainingSession trainingSession);

		Task<IList<TrainingType>> GetAllTrainingTypes();

	}
}
