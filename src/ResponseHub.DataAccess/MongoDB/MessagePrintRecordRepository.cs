using MongoDB.Driver;
using Enivate.ResponseHub.DataAccess.Interface;
using Enivate.ResponseHub.Model.Messages;
using System;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.DataAccess.MongoDB
{
    [MongoCollectionName("print_records")]
    public class MessagePrintRecordRepository : MongoRepository<MessagePrintRecord>, IMessagePrintRecordRepository
    {
        public async Task<MessagePrintRecord> GetPrintRecordByJobId(Guid jobId)
        {
            return await FindOne(i => i.JobMessageId == jobId);
        }

        public async Task<bool> PrintRecordExistsForJob(Guid jobId)
        {

            FilterDefinition<MessagePrintRecord> filter = Builders<MessagePrintRecord>.Filter.Eq(i => i.JobMessageId, jobId);

            long count = await Collection.CountDocumentsAsync(filter);

            return count > 0;
        }

        public async Task<MessagePrintRecord> CreatePrintRecord(Guid jobId, DateTime timestamp)
        {
            return await Add(new MessagePrintRecord()
            {
                JobMessageId = jobId,
                DatePrinted = timestamp
            });
        }
    }
}
