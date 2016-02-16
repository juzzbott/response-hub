using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class GroupService : IGroupService
	{

		private IGroupRepository _repository;

		private IRegionRepository _regionRepository;

		/// <summary>
		/// Creates a new instance of the Group application service.
		/// </summary>
		/// <param name="repository">The repository used to persist group data.</param>
		public GroupService(IGroupRepository repository, IRegionRepository regionRepository)
		{
			_repository = repository;
			_regionRepository = regionRepository;
		}

		/// <summary>
		/// Creates a new group object.
		/// </summary>
		/// <param name="name">The name of the group.</param>
		/// <param name="service">The service the group belongs to.</param>
		/// <returns>The created group object.</returns>
		public async Task<Group> CreateGroup(string name, ServiceType service, string capcode, Guid groupAdministratorId, string description, Region region)
		{
			Group group = new Group()
			{
				Name = name,
				Created = DateTime.UtcNow,
				Service = service,
				Description = description,
				Region = region
			};

			return await _repository.CreateGroup(group);

		}

		/// <summary>
		/// Gets all the groups in the repository.
		/// </summary>
		/// <returns></returns>
		public async Task<IList<Group>> GetAll()
		{
			return await _repository.GetAll();
		}

		/// <summary>
		/// Gets the most recently added groups in the system.
		/// </summary>
		/// <param name="count">The limit of results to return from the database query.</param>
		/// <returns>The most recent groups found.</returns>
		public async Task<IList<Group>> GetRecentlyAdded(int count)
		{

			if (count < 1)
			{
				throw new ArgumentOutOfRangeException("count", "The count parameter must be a positive integer.");
			}

			return await _repository.GetRecentlyAdded(count);
		}

		public async Task<Group> GetById(Guid id)
		{
			return await _repository.GetById(id);
		}

		public async Task<bool> CheckIfGroupExists(string name, ServiceType service)
		{
			return await _repository.CheckIfGroupExists(name, service);
		}

		public async Task AddUserToGroup(Guid userId, string role, Guid groupId)
		{
			UserMapping mapping = new UserMapping()
			{
				Role = role,
				UserId = userId
			};

			await _repository.AddUserToGroup(mapping, groupId);
		}

		public async Task<IList<Region>> GetRegions()
		{
			return await _regionRepository.GetAll();
		}
	}
}
