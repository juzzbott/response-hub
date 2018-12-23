using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.AspNet.Identity;
using Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Model.Identity.Interface;
using Enivate.ResponseHub.Model.Identity;
using System.Net.Http.Headers;
using System.Configuration;

namespace Enivate.ResponseHub.UI.Controllers.Api
{
    public class BaseApiController : ApiController
    {

		private ILogger _log;
		protected ILogger Log
		{
			get
			{
				return _log ?? (_log = UnityConfiguration.Container.Resolve<ILogger>());
			}
		}

		private IUserService _userService;
		protected IUserService UserService
		{
			get
			{
				return _userService ?? (_userService = UnityConfiguration.Container.Resolve<IUserService>());
			}
		}

		public Guid UserId { get; set; }

		public IdentityUser CurrentUser { get; set; }

		public BaseApiController()
		{
			// If the user is authenticated, then get the details
			if (User.Identity.IsAuthenticated)
			{
				UserId = new Guid(User.Identity.GetUserId());
			}
		}

		public async Task<IdentityUser> GetCurrentUser()
		{
			return await UserService.FindByIdAsync(UserId);
		}

		public void ThrowResponseException(HttpStatusCode statusCode)
		{
			ThrowResponseException(statusCode, null);
		}

		public void ThrowResponseException(HttpStatusCode statusCode, string messageContent)
		{
			HttpResponseMessage message = new HttpResponseMessage(statusCode);
			if (!String.IsNullOrEmpty(messageContent))
			{
				message.Content = new StringContent(messageContent);
			}
			throw new HttpResponseException(message);
		}

        public async Task<bool> ValidateApiKeyHeader()
        {
            // Get the authHeader
            AuthenticationHeaderValue authHeader = Request.Headers.Authorization;

            string apiKey = ConfigurationManager.AppSettings["ResponseHubService.ApiKey"];

            // If the api key is null or empty, log error message and return not authorized
            if (String.IsNullOrWhiteSpace(apiKey))
            {
                await _log.Error("The ResponseHub service API key is invalid.");
                return false;
            }

            // If there is no auth header, or it's no of type APIKEY with matching Api key, then throw not authorized.
            if (authHeader == null || !authHeader.Scheme.Equals("APIKEY", StringComparison.CurrentCultureIgnoreCase) || !authHeader.Parameter.Equals(apiKey))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
	}
}
