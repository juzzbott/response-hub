using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Warnings;
using Enivate.ResponseHub.Model.Warnings.Interface;
using Enivate.ResponseHub.UI.Models.WeatherCentre;
using Enivate.ResponseHub.Model.WeatherData.Interface;
using Enivate.ResponseHub.Common.Configuration;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("weather-centre")]
    public class WeatherCentreController : BaseController
	{

		protected IWarningService WarningService = ServiceLocator.Get<IWarningService>();
		protected IWeatherDataService WeatherDataService = ServiceLocator.Get<IWeatherDataService>();

		[Route]
        // GET: WeatherCentre
        public async Task<ActionResult> Index()
        {

			// Create the model
			WeatherCentreViewModel model = new WeatherCentreViewModel();

			// Get the warnings
			try
			{
				IList<IWarning> warnings = WarningService.GetWarnings(WarningSource.CountryFireAuthority | WarningSource.StateEmergencyService);
			}
			catch (Exception ex)
			{
				await Log.Error(String.Format("Error loading warnings. Message: {0}", ex.Message), ex);
				ViewBag.LoadWarningsError = true;
			}

			// Get the weather data location
			WeatherLocationElement location = ConfigurationSettings.WeatherData.Locations[0];

			// Create the rain radar
			RadarLoopViewModel rainRadar = new RadarLoopViewModel()
			{
				RadarBaseCode = location.BaseRadarProductId,
				RadarCode = location.RainRadarProductId,
				RadarImageFiles = WeatherDataService.GetRadarImagesForProduct(location.RainRadarProductId)
			};
			model.RainRadar = rainRadar;

			// Create the wind radar
			RadarLoopViewModel windRadar = new RadarLoopViewModel()
			{
				RadarBaseCode = location.BaseRadarProductId,
				RadarCode = location.WindRadarProductId,
				RadarImageFiles = WeatherDataService.GetRadarImagesForProduct(location.WindRadarProductId)
			};
			model.WindRadar = windRadar;


			return View(model);
        }
    }
}