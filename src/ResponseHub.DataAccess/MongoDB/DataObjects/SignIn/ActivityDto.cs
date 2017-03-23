using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson.Serialization.Attributes;

namespace Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.SignIn
{

	[BsonKnownTypes(typeof(OperationActivityDto), typeof(TrainingActivityDto))]
	public abstract class ActivityDto
	{
	}
}
