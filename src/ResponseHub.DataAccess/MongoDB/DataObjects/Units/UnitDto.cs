using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Units;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Units
{
	public class UnitDto : IEntity
	{

		public Guid Id { get; set; }

		public string Capcode { get; set; }

		public IList<Guid> AdditionalCapcodes { get; set; }

		public string Name { get; set; }

		public DateTime Created { get; set; }

		public DateTime Updated { get; set; }

		public ServiceType Service { get; set; }

		public string Description { get; set; }

		public IList<UserMapping> Users { get; set; }

		public Guid RegionId { get; set; }

		public GeoJson2DGeographicCoordinates HeadquartersCoordinates { get; set; }

		public TrainingNightInfo TrainingNight { get; set; }

		public UnitDto()
		{
			Id = Guid.NewGuid();
			Users = new List<UserMapping>();
			AdditionalCapcodes = new List<Guid>();
		}

	}
}
