using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enivate.ResponseHub.Model;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;

namespace Enivate.ResponseHub.PagerDecoder.Tests.Mocks
{
    public class MockJobMessageService : IJobMessageService
    {
        public Task AddAdditionalMessages(IList<KeyValuePair<Guid, AdditionalMessage>> additionalMessages)
        {
            throw new NotImplementedException();
        }

        public Task AddAttachmentToJob(Guid jobMessageId, Guid attachmentId)
        {
            throw new NotImplementedException();
        }

        public Task AddMessages(IList<JobMessage> messages)
        {
            throw new NotImplementedException();
        }

        public Task<JobNote> AddNoteToJobMessage(Guid jobMessageId, string noteBody, bool isWordBack, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResultSet<JobMessage>> FindByKeyword(string keyword, IEnumerable<string> capcodes, MessageType messageTypes, DateTime dateFrom, DateTime dateTo, int limit, int skip, bool countTotal)
        {
            throw new NotImplementedException();
        }

        public Task<JobMessage> GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IList<JobMessage>> GetByIds(IEnumerable<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public Task<JobMessage> GetByJobNumber(string jobNumber)
        {
            throw new NotImplementedException();
        }

        public Task<IList<JobMessage>> GetByUserId(Guid userId, int count, int skip)
        {
            throw new NotImplementedException();
        }

        public Task<IList<JobCode>> GetJobCodes()
        {

            return Task.Run(() =>
            {

                IList<JobCode> jobCodes = new List<JobCode>
                {
                new JobCode {
                    Id = 1,
                    ShortCode = "INCI",
                    IncidentType = "Incident",
                    RegexPattern = "(\\sINCIC\\d\\s)",
                    Sort = 1
                },
                new JobCode {
                    Id = 2,
                    ShortCode = "STRU",
                    IncidentType = "Structure Fire",
                    RegexPattern = "(\\sSTRUC\\d\\s)",
                    Sort = 1
                },
                new JobCode {
                    Id = 3,
                    ShortCode = "G&S",
                    IncidentType = "Grass & Scrub",
                    RegexPattern = "(\\sG\\&SC\\d\\s)",
                    Sort = 1
                },
                new JobCode {
                    Id = 4,
                    ShortCode = "NOST",
                    IncidentType = "Non-Structure Fire",
                    RegexPattern = "(\\sNOSTC\\d\\s)",
                    Sort = 1
                },
                new JobCode {
                    Id = 5,
                    ShortCode = "ALAR",
                    IncidentType = "Alarm",
                    RegexPattern = "(\\sALARC\\d\\s)",
                    Sort = 1
                },
                new JobCode {
                    Id = 6,
                    ShortCode = "RESC",
                    IncidentType = "Rescue",
                    RegexPattern = "(\\sRESCC\\d\\s|\\sRESC\\s)",
                    Sort = 1
                },
                new JobCode {
                    Id = 7,
                    ShortCode = "ASSIST POLICE",
                    IncidentType = "Assist Police",
                    RegexPattern = "(\\sASSIST POLICE\\s)",
                    Sort = 1
                },
                new JobCode {
                    Id = 8,
                    ShortCode = "ASSIST AV",
                    IncidentType = "Assist Ambulance",
                    RegexPattern = "(\\sASSIST AV\\s)|(\\sASSIST AMBULANCE\\s)",
                    Sort = 1
                },
                new JobCode {
                    Id = 9,
                    ShortCode = "FLOOD",
                    IncidentType = "Flood",
                    RegexPattern = "(\\sFLOOD\\s)",
                    Sort = 1
                },
                new JobCode {
                    Id = 10,
                    ShortCode = "TREE DWN / TRF HZD",
                    IncidentType = "Tree Down / Traffic Hazard",
                    RegexPattern = "(\\sTREE DOWN\\s?\\/\\s?TRF HAZARD\\s)",
                    Sort = 1
                },
                new JobCode {
                    Id = 11,
                    ShortCode = "BLD DMG",
                    IncidentType = "Building Damage",
                    RegexPattern = "(\\sBUILDING DAMAGE\\s)",
                    Sort = 1
                },
                new JobCode {
                    Id = 12,
                    ShortCode = "TREE DOWN",
                    IncidentType = "Tree Down",
                    RegexPattern = "(\\sTREE DOWN\\s)",
                    Sort = 2
                },
                new JobCode {
                    Id = 13,
                    ShortCode = "ANIMAL INCIDENT",
                    IncidentType = "Animal Incident",
                    RegexPattern = "(\\sANIMAL INCIDENT\\s)",
                    Sort = 1
                },
                new JobCode {
                    Id = 14,
                    ShortCode = "STCO",
                    IncidentType = "Structure Collapse",
                    RegexPattern = "(\\sSTCOC\\d\\s|\\sSTCO\\s)",
                    Sort = 1
                }
                };

                return jobCodes;

            });

        }

        public Task<IList<KeyValuePair<string, Guid>>> GetJobMessageIdsFromCapcodeJobNumbers(IList<KeyValuePair<string, string>> capcodeJobNumbers)
        {
            throw new NotImplementedException();
        }

        public Task<IList<JobMessage>> GetLatestFromLastMessage(Guid lastId, IEnumerable<string> capcodes, MessageType messageTypes)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Guid>> GetMessageIdsBetweenDates(IEnumerable<string> capcodes, MessageType messageTypes, DateTime? dateFrom, DateTime? dateTo)
        {
            throw new NotImplementedException();
        }

        public Task<IList<JobMessage>> GetMessagesBetweenDates(IEnumerable<string> capcodes, MessageType messageTypes, int count, int skip, DateTime? dateFrom, DateTime? dateTo)
        {
            throw new NotImplementedException();
        }

        public Task<IList<JobMessage>> GetMessagesBetweenDates(MessageType messageTypes, int count, int skip, DateTime? dateFrom, DateTime? dateTo)
        {
            throw new NotImplementedException();
        }

        public Task<IList<JobMessage>> GetMostRecent(IEnumerable<string> capcodes, MessageType messageTypes, int count, int skip)
        {
            throw new NotImplementedException();
        }

        public Task<IList<JobMessage>> GetMostRecent(MessageType messageTypes, int count, int skip)
        {
            throw new NotImplementedException();
        }

        public Task<IList<JobMessage>> GetMostRecent(IEnumerable<string> capcodes, int count, int skip)
        {
            throw new NotImplementedException();
        }

        public Task<IList<JobMessage>> GetMostRecent(int count, int skip)
        {
            throw new NotImplementedException();
        }

        public Task<IList<JobMessage>> GetMostRecent(Guid lastId)
        {
            throw new NotImplementedException();
        }

        public Task<IList<JobNote>> GetNotesForJob(Guid jobMessageId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAttachmentFromJob(Guid jobMessageId, Guid attachmentId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveProgress(Guid jobMessageId, MessageProgressType progressType)
        {
            throw new NotImplementedException();
        }

        public Task<MessageProgress> SaveProgress(Guid jobMessageId, DateTime progressDateTime, Guid userId, MessageProgressType progressType)
        {
            throw new NotImplementedException();
        }
    }
}
