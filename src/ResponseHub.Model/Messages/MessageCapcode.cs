using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Model.Messages
{
    public class MessageCapcode
    {

        public string Capcode { get; set; }

        public MessagePriority Priority { get; set; }

        public MessageCapcode()
        {
            // Default to administration.
            Priority = MessagePriority.Administration;
        }

        public MessageCapcode(string capcode, MessagePriority priority)
        {
            Capcode = capcode;
            Priority = priority;
        }

    }
}
