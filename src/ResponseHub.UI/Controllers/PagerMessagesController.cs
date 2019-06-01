using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Unity;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Units;
using Enivate.ResponseHub.Model.Units.Interface;
using Enivate.ResponseHub.UI.Models.PagerMessages;

namespace Enivate.ResponseHub.UI.Controllers
{

	[RoutePrefix("pager-messages")]
    public class PagerMessagesController : BaseController
    {

        protected readonly ICapcodeService CapcodeService = ServiceLocator.Get<ICapcodeService>();
        protected readonly IJobMessageService JobMessageService = ServiceLocator.Get<IJobMessageService>();
        protected readonly IUnitService UnitService = ServiceLocator.Get<IUnitService>();

        // GET: AllPages
        [Route()]
        public async Task<ActionResult> Index()
        {

			// Get the 50 most recent messages
			IList<JobMessage> messages = await JobMessageService.GetMostRecent(50, 0);
            IList<JobMessage> mapMessages = await JobMessageService.GetMostRecent(MessageType.Job, 100, 0);

            PagerMessageListViewModel model = new PagerMessageListViewModel()
            {
                LatestMessages = messages,
                MapMessages = mapMessages.Where(i => i.Location != null && i.Location.Coordinates != null).ToList()
            };

            return View(model);
        }

        [Route("{id:guid}")]
        public async Task<ActionResult> ViewPagerMessage(Guid id)
        {
            // Get the job message from the database
            JobMessage job = await JobMessageService.GetById(id);

            // If the job is null, return 404
            if (job == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "The requested page cannot be found.");
            }

            try
            {
                
                // Create the model object.
                PagerMessageViewModel model = await MapPagerMessageToViewModel(job);

                // return the job view
                return View(model);

            }
            catch (Exception ex)
            {
                // Log the error
                await Log.Error(String.Format("Unable to load the job details. Message: {0}", ex.Message, ex));
                return View(new object());

            }
        }

        private async Task<PagerMessageViewModel> MapPagerMessageToViewModel(JobMessage job)
        {

            // Get the capcodes for the job
            ICapcodeService capcodeService = ServiceLocator.Get<ICapcodeService>();
            IList<Capcode> jobCapcodes = await capcodeService.GetByCapcodeAddress(job.Capcodes.Select(i => i.Capcode));

            PagerMessageViewModel model = new PagerMessageViewModel()
            {
                Id = job.Id,
                JobNumber = job.JobNumber,
                Location = job.Location,
                MessageBody = job.MessageContent,
                AdditionalMessages = job.AdditionalMessages,
                Priority = job.Capcodes.OrderBy(i => i.Priority).FirstOrDefault().Priority,
                Timestamp = job.Timestamp.ToLocalTime(),
                Version = job.Version,
                JobCapcodes = jobCapcodes
            };

            // return the mapped job view model
            return model;
        }
    }
}