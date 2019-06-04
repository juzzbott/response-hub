using Enivate.ResponseHub.Common.Extensions;
using Enivate.ResponseHub.DataAccess.MongoDB.DataObjects.Messages;
using Enivate.ResponseHub.Model.Messages;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.PagerDecoder.ConsoleRunner
{
    public class JobCodeProcessor
    {

        public async Task Process()
        {
            // Change the mongo defaults to use standard binary guid instead of legacy guid.
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

            // Load the configuration to read the connection string
            MongoUrl mongoUrl = new MongoUrl(ConfigurationManager.ConnectionStrings["MongoServer"].ConnectionString);

            // Specify the mongo client, db and collection objects.
            MongoClient mongoClient = new MongoClient(mongoUrl);
            IMongoDatabase mongoDb = mongoClient.GetDatabase(mongoUrl.DatabaseName);

            IMongoCollection<JobMessageDto> jobMessagesCollection = mongoDb.GetCollection<JobMessageDto>("job_messages");
            IMongoCollection<JobCode> jobCodesCollection = mongoDb.GetCollection<JobCode>("job_codes");
            IList<JobCode> allJobCodes = await jobCodesCollection.Find<JobCode>(new BsonDocument()).ToListAsync();

            FilterDefinition<JobMessageDto> filter = FilterDefinition<JobMessageDto>.Empty;
            FindOptions<JobMessageDto> options = new FindOptions<JobMessageDto>
            {
                BatchSize = 2,
                NoCursorTimeout = false
            };

            int count = 0;

            using (IAsyncCursor<JobMessageDto> cursor = await jobMessagesCollection.FindAsync(filter, options))
            {
                while (await cursor.MoveNextAsync())
                {
                    IEnumerable<JobMessageDto> batch = cursor.Current;
                    foreach (JobMessageDto document in batch)
                    {
                        count++;

                        JobCodeType jobCodeType = GetJobCodeFromMessage(document.MessageContent, allJobCodes);
                        
                        FilterDefinition<JobMessageDto> updateFilter = Builders<JobMessageDto>.Filter.Eq(i => i.Id, document.Id);

                        // Create the update
                        UpdateDefinition<JobMessageDto> update = Builders<JobMessageDto>.Update.Set(i => i.JobCode, jobCodeType);

                        // Do the update
                        await jobMessagesCollection.UpdateOneAsync(updateFilter, update);

                        Console.WriteLine(String.Format("{0}--{1}--{2}", count, jobCodeType.ToString(), document.Id.ToString()));
                    }
                }
            }
        }

        public JobCodeType GetJobCodeFromMessage(string message, IList<JobCode> jobCodes)
        {
            string jobShortCode = "";
            foreach (JobCode jobCode in jobCodes)
            {
                if (Regex.IsMatch(message, jobCode.RegexPattern))
                {
                    jobShortCode = jobCode.ShortCode;
                    break;
                }
            }

            JobCodeType jobCodeType = JobCodeType.UKN;

            if (!String.IsNullOrEmpty(jobShortCode))
            {
                JobCode jobCode = jobCodes.FirstOrDefault(i => i.ShortCode.Equals(jobShortCode, StringComparison.CurrentCultureIgnoreCase));
                jobCodeType = (JobCodeType)jobCode.Id;
            }

            return jobCodeType;
        }

    }
}
