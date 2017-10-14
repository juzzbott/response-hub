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

		Task<Asset> GetAssetById(Guid id);

		Task<IList<Asset>> GetAssetsByUnitId(Guid unitId);

	}
}
