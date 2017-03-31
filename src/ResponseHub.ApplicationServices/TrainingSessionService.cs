using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Model.Training;
using Enivate.ResponseHub.Model.Training.Interface;
using Enivate.ResponseHub.DataAccess.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class TrainingSessionService : ITrainingSessionService
	{

		public ITrainingSessionRepository _repository;

		public TrainingSessionService(ITrainingSessionRepository repository)
		{
			_repository = repository;
		}

		/// <summary>
		/// Creates a new training session.
		/// </summary>
		/// <param name="trainingSession"></param>
		/// <returns></returns>
		public async Task CreateTrainingSession(TrainingSession trainingSession)
		{
			await _repository.Add(trainingSession);
		}

		/// <summary>
		/// Gets the list of training sessions for the specific group.
		/// </summary>
		/// <param name="groupId">The id of the group to get the training sessions for.</param>
		/// <returns>The list of training sessions.</returns>
		public async Task<IList<TrainingSession>> GetTrainingSessionsForGroup(Guid groupId)
		{
			return await _repository.GetTrainingSessionsForGroup(groupId);
		}

		/// <summary>
		/// Gets the list of training sessions for the specific group.
		/// </summary>
		/// <param name="groupId">The id of the group to get the training sessions for.</param>
		/// <returns>The list of training sessions.</returns>
		public async Task<IList<TrainingSession>> GetTrainingSessionsForGroup(Guid groupId, int limit)
		{
			return await _repository.GetTrainingSessionsForGroup(groupId);
		}

		public async Task<TrainingSession> GetById(Guid id)
		{
			return await _repository.GetById(id);
		}
		
		public async Task SaveTrainingSession(TrainingSession trainingSession)
		{
			await _repository.Save(trainingSession);
		}
	}
}
