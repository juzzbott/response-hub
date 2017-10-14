using Enivate.ResponseHub.Model.Upkeep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.DataAccess.Interface
{
	public interface IUpkeepRepository
	{

		Task AddAsset(Asset asset);

		Task<Asset> GetAssetById(Guid id);

		Task<IList<Asset>> GetAssetsByUnitId(Guid unitId);
	}
}
