using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Messages
{
    public class JobCode
    {

        public int Id { get; set; }

        public string ShortCode { get; set; }

        public string IncidentType { get; set; }

        public string RegexPattern { get; set; }

        public int Sort { get; set; }

    }
}
