using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

using Enivate.ResponseHub.Common;

namespace Enivate.ResponseHub.Tests.Integration.Fixtures
{
	public class UnityCollectionFixture : IDisposable
	{

		public UnityCollectionFixture()
		{
			// Unity configuration loader
			UnityConfiguration.Container = new UnityContainer().LoadConfiguration();
		}

		public void Dispose()
		{

		}

	}
}
