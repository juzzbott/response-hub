using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

using Microsoft.Practices.Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.UI.Controllers.Api
{

	[RoutePrefix("api/pager-messages")]
    public class PagerMessageController : ApiController
    {

		private ILogger _log;
		protected ILogger Log
		{
			get
			{
				return _log ?? (_log = UnityConfiguration.Container.Resolve<ILogger>());
			}
		}

		private IPagerMessageService _pagerMessageService;
		protected IPagerMessageService PagerMessageService
		{
			get
			{
				return _pagerMessageService ?? (_pagerMessageService = UnityConfiguration.Container.Resolve<IPagerMessageService>());
			}
		}

		[Route]
		[HttpPost]
		public bool Post(PagerMessage pagerMessage)
		{

			// Get the authHeader
			AuthenticationHeaderValue authHeader = Request.Headers.Authorization;

			string apiKey = ConfigurationManager.AppSettings["ResponseHubService.ApiKey"];

			// If the api key is null or empty, log error message and return not authorized
			if (String.IsNullOrWhiteSpace(apiKey))
			{
				Log.Error("The ResponseHub service API key is invalid.");
				throw new HttpResponseException(HttpStatusCode.Unauthorized);
			}

			// If there is no auth header, or it's no of type APIKEY with matching Api key, then throw not authorized.
			if (authHeader == null || !authHeader.Scheme.Equals("APIKEY", StringComparison.CurrentCultureIgnoreCase) || !authHeader.Parameter.Equals(apiKey))
			{
				throw new HttpResponseException(HttpStatusCode.Unauthorized);
			}

			// Save the pager message
			_pagerMessageService.Save(pagerMessage);

			return true;
		}

    }
}
