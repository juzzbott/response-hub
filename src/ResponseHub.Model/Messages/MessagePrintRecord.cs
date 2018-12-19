using System;

namespace Enivate.ResponseHub.Model.Messages
{
    public class MessagePrintRecord : IEntity
    {

        public Guid Id { get; set; }

        public Guid JobMessageId { get; set; }

        public DateTime DatePrinted { get; set; }

        public MessagePrintRecord()
        {
            Id = Guid.NewGuid();
        }

    }
}
