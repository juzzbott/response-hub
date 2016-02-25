using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.Model.Identity;

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
		public async Task<Group> CreateGroup(string name, ServiceType service, string capcode, Guid groupAdministratorId, string description, Region region, Coordinates headquartersCoords)
		{
			Group group = new Group()
			{
				Name = name,
				Created = DateTime.UtcNow,
				Updated = DateTime.UtcNow,
				Service = service,
				Capcode = capcode,
				Description = description,
				Region = region,
				HeadquartersCoordinates = headquartersCoords
			};

			// Add the user mapping for the group administrator
			group.Users.Add(new UserMapping()
			{
				Role = RoleTypes.GroupAdministrator,
				UserId = groupAdministratorId
			});

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

		/// <summary>
		/// Gets the specific group from the ID.
		/// </summary>
		/// <param name="id">The ID of the group to return.</param>
		/// <returns>The group is found by ID, otherwise null.</returns>
		public async Task<Group> GetById(Guid id)
		{
			return await _repository.GetById(id);
		}

		/// <summary>
		/// Determines if the group name exists for the specified service already within the system.
		/// </summary>
		/// <param name="name">The name of the group.</param>
		/// <param name="service">The service type to check.</param>
		/// <returns>True if the group name exists, otherwise false.</returns>
		public async Task<bool> CheckIfGroupExists(string name, ServiceType service)
		{
			return await _repository.CheckIfGroupExists(name, service);
		}

		/// <summary>
		/// Adds the specified user to the group.
		/// </summary>
		/// <param name="userId">The ID of the user to add to the group.</param>
		/// <param name="role">The role of the user within the group.</param>
		/// <param name="groupId">The Id of the group to add the user to.</param>
		/// <returns></returns>
		public async Task AddUserToGroup(Guid userId, string role, Guid groupId)
		{
			UserMapping mapping = new UserMapping()
			{
				Role = role,
				UserId = userId
			};

			await _repository.AddUserToGroup(mapping, groupId);
		}

		/// <summary>
		/// Gets all the regions that a group can be a member of.
		/// </summary>
		/// <returns></returns>
		public async Task<IList<Region>> GetRegions()
		{
			return await _regionRepository.GetAll();
		}

		/// <summary>
		/// Finds groups by name. This is a text based search and will match any of the words in the group name.
		/// </summary>
		/// <param name="name">The name of the group to search for.</param>
		/// <returns>The list of groups that match against the group name.</returns>
		public async Task<IList<Group>> FindByName(string name)
		{
			return await _repository.FindByName(name);
		}

		/// <summary>
		/// Updates the group in the database.
		/// </summary>
		/// <param name="group">The group to update in the database.</param>
		/// <returns></returns>
		public async Task UpdateGroup(Group group)
		{

			// If the group is null or the group id is empty guid, throw exception as the group should be saved first.
			if (group == null || group.Id == Guid.Empty)
			{
				throw new Exception("The group must exist before it can be updated.");
			}

			// Save the group to the database.
			await _repository.UpdateGroup(group);

		}
	}
}
