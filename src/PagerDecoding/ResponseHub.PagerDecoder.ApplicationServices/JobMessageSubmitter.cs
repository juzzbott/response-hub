using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

using Enivate.ResponseHub.Model.Messages;

using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace Enivate.ResponseHub.PagerDecoder.ApplicationServices
{
	public class JobMessageSubmitter
	{


		/// <summary>
		/// The configuration key for the web service url.
		/// </summary>
		private static string _webServiceUrlKey = "ResponseHubService.Url";

		/// <summary>
		/// The configuration key for the web service api key.
		/// </summary>
		private static string _webServiceUrlApiKeyKey = "ResponseHubService.ApiKey";

		/// <summary>
		/// Posts the messages to the webservice
		/// </summary>
		/// <returns></returns>
		public static bool PostJobMessagesToWebService(Dictionary<string, JobMessage> jobMessagesToSubmit)
		{
			// Get the json string for the list of pager messages
			string jsonData = JsonConvert.SerializeObject(jobMessagesToSubmit.Select(i => i.Value).ToArray());
			byte[] jsonBytes = Encoding.ASCII.GetBytes(jsonData);

			// Get the service url and api key
			string serviceUrl = ConfigurationManager.AppSettings[_webServiceUrlKey];
			string serviceApiKey = ConfigurationManager.AppSettings[_webServiceUrlApiKeyKey];

			// If the service url is null or empty, throw exception
			if (String.IsNullOrEmpty(serviceUrl))
			{
				throw new ApplicationException("The web service url configuration is missing or empty.");
			}

			// Create the post request
			HttpWebRequest request = WebRequest.CreateHttp(serviceUrl);
			request.Method = "POST";
			request.ContentType = "application/json";
			request.ContentLength = jsonBytes.Length;
			request.Headers.Add(HttpRequestHeader.Authorization, String.Format("APIKEY {0}", serviceApiKey));
			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(jsonBytes, 0, jsonBytes.Length);
			}

			// Create the message sha variable
			string responseText = "";

			// Get the response
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			// If all went well, set the last message sha to the last in the list of jbo messages
			if (response.StatusCode == HttpStatusCode.OK)
			{
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{
					responseText = reader.ReadToEnd();
				}
			}

			// return the message sha
			return (responseText.ToLower() == "true");

		}

	}
}
