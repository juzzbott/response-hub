using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Common;

using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace Enivate.ResponseHub.PagerDecoder.Tests.Fixtures
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
