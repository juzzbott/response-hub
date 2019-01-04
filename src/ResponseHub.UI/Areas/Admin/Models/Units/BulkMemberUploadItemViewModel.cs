using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.Admin.Models.Units
{
    public class BulkMemberUploadItemViewModel
    {

        public string EmailAddress { get; set; }

        public string FirstName { get; set; }

        public string Surname { get; set; }

        public string MemberNumber { get; set; }

        public string UserAccessType { get; set; }

        /// <summary>
        /// Determine if the bulk user item is valid or not.
        /// </summary>
        /// <returns></returns>
        public string IsValid()
        {
            // Validate email exists
            if (string.IsNullOrEmpty(EmailAddress))
            {
                return "Email address is not defined.";
            }

            // Validate email address is valid
            if (!EmailAddress.Contains("@"))
            {
                return "Email address appears to be invalid.";
            }

            // Validate first name exists
            if (string.IsNullOrEmpty(FirstName))
            { 
                return "First name is not defined.";
            }

            // Validate surname exists
            if (string.IsNullOrEmpty(Surname))
            {
                return "Surname is not defined.";
            }

            // Validate member number exists
            if (string.IsNullOrEmpty(MemberNumber))
            {
                return "Member number is not defined.";
            }

            // Validate surname exists
            if (string.IsNullOrEmpty(UserAccessType))
            {
                return "User access level is not defined.";
            }

            // Validate surname exists
            if (!UserAccessType.Equals("General user", StringComparison.CurrentCultureIgnoreCase) && !UserAccessType.Equals("Unit administrator", StringComparison.CurrentCultureIgnoreCase))
            {
                return "User access level is neither 'General user' or 'Unit administrator'.";
            }

            // return no error message
            return "";
        }

    }
}