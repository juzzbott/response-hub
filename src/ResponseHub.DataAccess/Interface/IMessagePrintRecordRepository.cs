using Enivate.ResponseHub.Model.Messages;
using System;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.DataAccess.Interface
{
    public interface IMessagePrintRecordRepository
    {

        Task<MessagePrintRecord> GetPrintRecordByJobId(Guid jobId);

        Task<bool> PrintRecordExistsForJob(Guid jobId);

        Task<MessagePrintRecord> CreatePrintRecord(Guid jobId, DateTime timestamp);


    }
}
