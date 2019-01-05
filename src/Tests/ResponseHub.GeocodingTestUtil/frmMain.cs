using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Logging;
using Enivate.ResponseHub.Wrappers.Google;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResponseHub.GeocodingTestUtil
{
    public partial class frmMain : Form
    {

        /// <summary>
        /// The configuration key containing the google geocode api key.
        /// </summary>
        private const string _googleGeocodeApiKeyConfigKey = "GoogleGeocodeApiKey";

        protected ILogger Log
        {
            get
            {
                return ServiceLocator.Get<ILogger>();
            }
        }

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

            // Set the tooltips
            toolTip.SetToolTip(lblLocationToolTip, "The address or location to query the Geocode API for.");
        }

        private async void btnGenerate_Click(object sender, EventArgs e)
        {
            // Set the progress bar
            prgGenerating.Style = ProgressBarStyle.Marquee;
            prgGenerating.MarqueeAnimationSpeed = 30;

            // Double check any spaces are removed from the query
            string addressQuery = txtAddress.Text;
            addressQuery = addressQuery.Replace(" ", "+");

            // Get the API key from the configuration
            string apiKey = ConfigurationManager.AppSettings[_googleGeocodeApiKeyConfigKey];

            // Get the service url. If the API Key is available, then use that.
            string serviceUrl = string.Format("https://maps.googleapis.com/maps/api/geocode/json?address={0}&key={1}", addressQuery, apiKey);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Create the client and request objects
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(serviceUrl, HttpCompletionOption.ResponseContentRead);
            sw.Stop();

            // Read the response data.
            string responseContent = await response.Content.ReadAsStringAsync();

            // if there is no json data, return failed.
            if (string.IsNullOrEmpty(responseContent))
            {
                // Log the empty Google API response.
                await Log.Warn(string.Format("Empty response returned from Google Maps API request. Requested URL: {0} | Response status: {1}", serviceUrl, response.StatusCode));

                // return a null result
                return;
            }

            // Log the response from Google Geocode Service
            await Log.Debug(string.Format("Found address data for address: {0}", addressQuery));

            // Set the status result
            lblResonseStatus.Text = string.Format("{0} {1}", (int)response.StatusCode, response.StatusCode.ToString());
            lblResponseTime.Text = String.Format("{0}ms", sw.ElapsedMilliseconds);

            // Set the response headers.
            StringBuilder responseHeaders = new StringBuilder();
            foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
            {
                responseHeaders.AppendLine(string.Format("{0}: {1}", header.Key, header.Value.ToArray()[0]));
            }
            txtResponseHeaders.Text = responseHeaders.ToString();
            string indentedJson = FormatJson(responseContent);
            txtResponseBody.Text = indentedJson.Replace("\t", "  ");

            // Display HTML result
            DisplayHtmlResponse(indentedJson);

            // Set the progress bar;
            prgGenerating.Style = ProgressBarStyle.Blocks;
            prgGenerating.MarqueeAnimationSpeed = 0;

        }

        private static string FormatJson(string json)
        {
            using (var stringReader = new StringReader(json))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) {
                    Formatting = Formatting.Indented,
                    Indentation = 3,
                    IndentChar = '\t'
                };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
        }

        public void DisplayHtmlResponse(string responseData)
        {
            // Generate the html header and footer
            string htmlDisplayHeader = "<html><head><style type=\"text/css\">body { font-size: 11px; font-family: Arial, sans-serif; color: #999 } .key {color: #18899c; } .number {color: #905; } .string { color: #690; } .boolean { color: #e23f90; } .null { color: #d79177; } p {margin-bottom: 5px; }</style></head><body>";
            string htmlDisplayFooter = "</body></html>";

            // Highlight Json
            string highlightedJson = SyntaxHighlightJson(responseData);

            // Ensure indenting by replating \r\n with <br />
            highlightedJson = highlightedJson.Replace("\r\n", "<br />\r\n");
            highlightedJson = highlightedJson.Replace("\t", "&nbsp;");

            // Set the web browser content
            browserFormattedJson.DocumentText = "0";
            browserFormattedJson.Document.OpenNew(true);
            browserFormattedJson.Document.Write(String.Format("{0}{1}{2}", htmlDisplayHeader, highlightedJson, htmlDisplayFooter));
            browserFormattedJson.Refresh();
        }


        public string SyntaxHighlightJson(string original)
        {
            return Regex.Replace(
              original,
              @"(¤(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\¤])*¤(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)".Replace('¤', '"'),
              match => {
                  var cls = "number";
                  if (Regex.IsMatch(match.Value, @"^¤".Replace('¤', '"')))
                  {
                      if (Regex.IsMatch(match.Value, ":$"))
                      {
                          cls = "key";
                      }
                      else
                      {
                          cls = "string";
                      }
                  }
                  else if (Regex.IsMatch(match.Value, "true|false"))
                  {
                      cls = "boolean";
                  }
                  else if (Regex.IsMatch(match.Value, "null"))
                  {
                      cls = "null";
                  }
                  return "<span class=\"" + cls + "\">" + match + "</span>";
              });
        }

        private void rdoFormattedJson_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoFormattedJson.Checked)
            {
                rdoRawResponse.Checked = false;
                browserFormattedJson.Visible = true;
                txtResponseBody.Visible = false;
            }
        }

        private void rdoRawResponse_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoRawResponse.Checked)
            {
                rdoFormattedJson.Checked = false;
                browserFormattedJson.Visible = false;
                txtResponseBody.Visible = true;
            }
        }
    }
}
