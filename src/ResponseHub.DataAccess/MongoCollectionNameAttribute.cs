using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.DataAccess
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class MongoCollectionNameAttribute : System.Attribute
	{

		public string CollectionName { get; set; }

		public MongoCollectionNameAttribute(string collectionName)
		{
			this.CollectionName = collectionName;
		}

	}
}
