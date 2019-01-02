using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Units
{
    public class BulkMemberUploadViewModel
    {

        public Guid UnitId { get; set; }

        public HttpPostedFileBase File { get; set; }
    }
}