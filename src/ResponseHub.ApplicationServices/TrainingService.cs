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
		/// Gets the list of training sessions for the specific unit.
		/// </summary>
		/// <param name="unitId">The id of the unit to get the training sessions for.</param>
		/// <returns>The list of training sessions.</returns>
		public async Task<IList<TrainingSession>> GetTrainingSessionsForUnit(Guid unitId, Guid? memberId)
		{
			return await _sessionRepository.GetTrainingSessionsForUnit(unitId, await GetAllTrainingTypes(), memberId);
		}

		/// <summary>
		/// Gets the list of training sessions for the specific unit.
		/// </summary>
		/// <param name="unitId">The id of the unit to get the training sessions for.</param>
		/// <returns>The list of training sessions.</returns>
		public async Task<IList<TrainingSession>> GetTrainingSessionsForUnit(Guid unitId, int limit, Guid? memberId)
		{
			return await _sessionRepository.GetTrainingSessionsForUnit(unitId, await GetAllTrainingTypes(), memberId);
		}

		/// <summary>
		/// Gets the list of training sessions for the specific unit between the specified date ranges.
		/// </summary>
		/// <param name="unitId">The id of the unit to get the training sessions for.</param>
		/// <param name="dateFrom">The date to get the training sessions from.</param>
		/// <param name="dateTo">The date to get the training sessions to.</param>
		/// <returns>The list of training sessions.</returns>
		public async Task<IList<TrainingSession>> GetTrainingSessionsForUnit(Guid unitId, DateTime dateFrom, DateTime dateTo, Guid? memberId)
		{
			return await _sessionRepository.GetTrainingSessionsForUnit(unitId, dateFrom, dateTo, await GetAllTrainingTypes(), memberId);
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

		#region Training Types

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

		/// <summary>
		/// Finds the training type based on the id.
		/// </summary>
		/// <param name="id">The Id of the training type to find. </param>
		/// <returns>The training type if found. </returns>
		public async Task<TrainingType> GetTrainingTypeById(Guid id)
		{
			// Find by the id
			return await _trainingTypeRepository.GetById(id);
		}

		/// <summary>
		/// Saves the training type to the database. If the training type does not already exist, it will be created. 
		/// </summary>
		/// <param name="trainingType">The training type to save to the database.</param>
		/// <returns></returns>
		public async Task SaveTrainingType(TrainingType trainingType)
		{
			// save the training type
			await _trainingTypeRepository.Save(trainingType);

			// Clear the training types from the cache
			CacheManager.RemoveItem(AllTrainingTypesCacheKey);
		}

		#endregion

	}
}
