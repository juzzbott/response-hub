using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Caching;
using Enivate.ResponseHub.Model.Training;
using Enivate.ResponseHub.Model.Training.Interface;
using Enivate.ResponseHub.DataAccess.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class TrainingService : ITrainingService
	{

		public ITrainingSessionRepository _sessionRepository;
		public ITrainingTypeRepository _trainingTypeRepository;

		private const string AllTrainingTypesCacheKey = "AllTrainingTypes";

		public TrainingService(ITrainingSessionRepository sessionRepository, ITrainingTypeRepository trainingTypeRepository)
		{
			_sessionRepository = sessionRepository;
			_trainingTypeRepository = trainingTypeRepository;
		}

		/// <summary>
		/// Creates a new training session.
		/// </summary>
		/// <param name="trainingSession"></param>
		/// <returns></returns>
		public async Task CreateTrainingSession(TrainingSession trainingSession)
		{
			await _sessionRepository.Add(trainingSession);
		}

		/// <summary>
		/// Gets the list of training sessions for the specific group.
		/// </summary>
		/// <param name="groupId">The id of the group to get the training sessions for.</param>
		/// <returns>The list of training sessions.</returns>
		public async Task<IList<TrainingSession>> GetTrainingSessionsForGroup(Guid groupId)
		{
			return await _sessionRepository.GetTrainingSessionsForGroup(groupId, await GetAllTrainingTypes());
		}

		/// <summary>
		/// Gets the list of training sessions for the specific group.
		/// </summary>
		/// <param name="groupId">The id of the group to get the training sessions for.</param>
		/// <returns>The list of training sessions.</returns>
		public async Task<IList<TrainingSession>> GetTrainingSessionsForGroup(Guid groupId, int limit)
		{
			return await _sessionRepository.GetTrainingSessionsForGroup(groupId, await GetAllTrainingTypes());
		}

		/// <summary>
		/// Gets the training session from the database.
		/// </summary>
		/// <param name="id">The ID of the training session.</param>
		/// <returns>The training session found based on the id.</returns>
		public async Task<TrainingSession> GetTrainingSessionById(Guid id)
		{
			return await _sessionRepository.GetById(id, await GetAllTrainingTypes());
		}
		
		/// <summary>
		/// Saves the training session into the database.
		/// </summary>
		/// <param name="trainingSession">The training session to save.</param>
		/// <returns></returns>
		public async Task SaveTrainingSession(TrainingSession trainingSession)
		{
			await _sessionRepository.Save(trainingSession);
		}

		/// <summary>
		/// Gets a list of all the training types in the database.
		/// </summary>
		/// <returns></returns>
		public async Task<IList<TrainingType>> GetAllTrainingTypes()
		{
			// Try and get the list from the cache
			IList<TrainingType> trainingTypes = CacheManager.GetItem<IList<TrainingType>>(AllTrainingTypesCacheKey);

			// If the item was not found in the cache, then get from the repository
			if (trainingTypes == null)
			{
				// Get from the database
				trainingTypes = await _trainingTypeRepository.GetAll();

				// If there are results, then add to the cache
				if (trainingTypes != null && trainingTypes.Count > 0)
				{
					CacheManager.AddItem(AllTrainingTypesCacheKey, trainingTypes, DateTime.Now.AddDays(1));
				}
			}

			// return the training types
			return trainingTypes;
		}
	}
}
