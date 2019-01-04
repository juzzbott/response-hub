using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Units
{
    public class BulkMemberImportedViewModel
    {

        public Guid UnitId { get; set; }

        public IList<KeyValuePair<BulkMemberUploadItemViewModel, string>> ErrorMembers { get; set; }

        public IList<BulkMemberUploadItemViewModel> ImportedMembers { get; set; }

        public IList<BulkMemberUploadItemViewModel> AddedToUnit { get; set; }

        public int TotalMembersProcessed { get; set; }

        public BulkMemberImportedViewModel()
        {
            ErrorMembers = new List<KeyValuePair<BulkMemberUploadItemViewModel, string>>();
            ImportedMembers = new List<BulkMemberUploadItemViewModel>();
            AddedToUnit = new List<BulkMemberUploadItemViewModel>();
        }

    }
}