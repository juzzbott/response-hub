using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace Enivate.ResponseHub.Common
{
    public class UnityConfiguration
    {

		private static IUnityContainer _container;
		public static IUnityContainer Container
		{
			get
			{
				if (_container == null)
					throw new NullReferenceException("The UnityConfiguration Container property must be initialised before it is referenced.");
				return _container;
			}
			set
			{
				_container = value;
			}
		}

	}
}
