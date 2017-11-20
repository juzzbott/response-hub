using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Upkeep.Interface
{
	public interface IUpkeepService
	{

		Task<Asset> CreateAsset(string name, string description, Guid unitId);

		Task<Asset> SaveAsset(Asset asset);

		Task<Asset> GetAssetById(Guid id);

		Task<IList<Asset>> GetAssetsByUnitId(Guid unitId);

		Task<UpkeepTask> CreateTask(string name, Guid unitId, Guid? assetId, IList<string> taskItems);

		Task<UpkeepTask> SaveTask(UpkeepTask task);

		Task<UpkeepTask> GetTaskById(Guid id);

		Task<IList<UpkeepTask>> GetTasksByUnitId(Guid unitId);

	}
}
