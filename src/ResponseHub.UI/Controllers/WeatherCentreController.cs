using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.RadarImages.Interface;
using Enivate.ResponseHub.Model.Warnings;
using Enivate.ResponseHub.Model.Warnings.Interface;
using Enivate.ResponseHub.UI.Models.WeatherCentre;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("weather-centre")]
    public class WeatherCentreController : BaseController
	{



		protected IWarningService WarningService
		{
			get
			{
				return ServiceLocator.Get<IWarningService>();
			}
		}

		protected IRadarImageService RadarImageService
		{
			get
			{
				return ServiceLocator.Get<IRadarImageService>();
			}
		}

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

			// Create the rain radar
			RadarLoopViewModel rainRadar = new RadarLoopViewModel()
			{
				RadarBaseCode = "IDR022",
				RadarCode = "IDR022",
				RadarImageFiles = RadarImageService.GetRadarImagesForProduct("IDR022")
			};
			model.RainRadar = rainRadar;

			// Create the wind radar
			RadarLoopViewModel windRadar = new RadarLoopViewModel()
			{
				RadarBaseCode = "IDR022",
				RadarCode = "IDR02I",
				RadarImageFiles = RadarImageService.GetRadarImagesForProduct("IDR02I")
			};
			model.WindRadar = windRadar;


			return View(model);
        }
    }
}