using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity;

namespace Enivate.ResponseHub.Common
{

	public class ServiceLocator
	{
		private const string HttpContextItemsKey = "ServiceLocatorServices";

		/// <summary>
		/// Locates the specified service type.
		/// </summary>
		/// <typeparam name="TService">The type of service to locate.</typeparam>
		/// <returns></returns>
		public static TService Get<TService>()
		{

			// Get the type of service, and ensure it's an interface
			Type serviceType = typeof(TService);
			if (!serviceType.IsInterface)
			{
				throw new ApplicationException("The type of 'TService' must be an interface");
			}

			// Set the default TService
			TService service = default(TService);

			// Get from unity
			service = UnityConfiguration.Container.Resolve<TService>();

			// return the service
			return service;
		}

	}

}
