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

        public IList<KeyValuePair<BulkMemberUploadItemViewModel, string>> InvalidUsers { get; set; }

        public IList<BulkMemberUploadItemViewModel> UsersToBeImported { get; set; }

        public IList<BulkMemberUploadItemViewModel> ExistingUnitMembers { get; set; }

        public IList<BulkMemberUploadItemViewModel> MembersToBeAddedToUnit { get; set; }

        public int TotalMembersInFile { get; set; }

        public bool FileUploaded { get; set; }

        public bool InvalidDataFileColumns { get; set; }

        public BulkMemberUploadViewModel()
        {
            InvalidUsers = new List<KeyValuePair<BulkMemberUploadItemViewModel, string>>();
            UsersToBeImported = new List<BulkMemberUploadItemViewModel>();
            ExistingUnitMembers = new List<BulkMemberUploadItemViewModel>();
            MembersToBeAddedToUnit = new List<BulkMemberUploadItemViewModel>();
        }
    }
}