using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Training;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface ITrainingSessionRepository : IRepository<TrainingSession>
	{

		Task<IList<TrainingSession>> GetTrainingSessionsForGroup(Guid groupId);

		Task<IList<TrainingSession>> GetTrainingSessionsForGroup(Guid groupId, int limit);

	}
}
