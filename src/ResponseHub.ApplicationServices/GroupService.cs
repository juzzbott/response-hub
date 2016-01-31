using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Groups.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class GroupService : IGroupService
	{

		private IGroupRepository _repository;

		/// <summary>
		/// Creates a new instance of the Group application service.
		/// </summary>
		/// <param name="repository">The repository used to persist group data.</param>
		public GroupService(IGroupRepository repository)
		{
			_repository = repository;
		}

		/// <summary>
		/// Creates a new group object.
		/// </summary>
		/// <param name="name">The name of the group.</param>
		/// <param name="service">The service the group belongs to.</param>
		/// <returns>The created group object.</returns>
		public async Task<Group> CreateGroup(string name, ServiceType service, string description)
		{
			Group group = new Group()
			{
				Name = name,
				Created = DateTime.UtcNow,
				Service = service,
				Description = description
			};

			return await _repository.CreateGroup(group);

		}
	}
}
