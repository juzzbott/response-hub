using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Enivate.ResponseHub.Model.Messages;

namespace Enivate.ResponseHub.UI.Models.PagerMessages
{
    public class PagerMessageListViewModel
    {

        public IList<JobMessage> LatestMessages { get; set; }

        public IList<JobMessage> MapMessages { get; set; }

        public PagerMessageListViewModel()
        {
            LatestMessages = new List<JobMessage>();
            MapMessages = new List<JobMessage>();
        }

    }
}