using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Enivate.ResponseHub.Common;
using Enivate.ResponseHub.Common.Constants;
using Enivate.ResponseHub.Model.DataExport.Interface;
using Enivate.ResponseHub.Model.Groups;
using Enivate.ResponseHub.Model.Messages;
using Enivate.ResponseHub.Model.Messages.Interface;
using Enivate.ResponseHub.Model.Spatial;
using Enivate.ResponseHub.Model.Groups.Interface;
using Enivate.ResponseHub.Model.PdfGeneration.Interface;

namespace Enivate.ResponseHub.ApplicationServices
{
	public class DataExportService : IDataExportService
	{

		private IJobMessageService _jobMessageService;
		private IGroupService _groupService;
		private IPdfGenerationService _pdfGenerationService;

		public DataExportService(IJobMessageService jobMessageService, IGroupService groupService, IPdfGenerationService pdfGenerationService)
		{
			_jobMessageService = jobMessageService;
			_groupService = groupService;
			_pdfGenerationService = pdfGenerationService;
		}
		
		public async Task<string> BuildCsvExportFile(Guid groupId, DateTime dateFrom, DateTime dateTo)
		{

			// Get the group by the id
			Group group = await _groupService.GetById(groupId);

			// Get the list of messages for the capcode
			IList<JobMessage> messages = await _jobMessageService.GetMessagesBetweenDates(
				new List<Capcode> { new Capcode() { CapcodeAddress = group.Capcode } },
				MessageType.Job & MessageType.Message,
				999999,
				0,
				dateFrom,
				dateTo);

			// Create the string builder to store the data in
			StringBuilder sb = new StringBuilder();

			// Append the header fields
			sb.AppendLine("Id,Capcode,JobNumber,Message,Timestamp,Priority,Map Reference,Gps Coordinates,On Route,On Scene,Job Clear");

			// Iterate through the job messages
			foreach (JobMessage message in messages)
			{
				sb.AppendLine(String.Format("{0},{1},{2},{3},\"{4}\",{5},{6},{7},{8},{9},{10}",
					message.Id,
					message.Capcode,
					message.JobNumber,
					message.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
					message.MessageContent,
					message.Priority,
					GetMapReference(message.Location),
					GetGpsCoordinates(message.Location),
					GetJobProgress(message, MessageProgressType.OnRoute),
					GetJobProgress(message, MessageProgressType.OnScene),
					GetJobProgress(message, MessageProgressType.JobClear)));
			}

			return sb.ToString();
		}

		/// <summary>
		/// Gets the job progress based on the progress type for the export file
		/// </summary>
		/// <param name="message"></param>
		/// <param name="progressType"></param>
		/// <returns></returns>
		private static string GetJobProgress(JobMessage message, MessageProgressType progressType)
		{

			// Variable to store the progress
			MessageProgress progress = null;

			switch (progressType)
			{
				case MessageProgressType.OnRoute:
					progress = message.ProgressUpdates.FirstOrDefault(i => i.ProgressType == MessageProgressType.OnRoute);
					break;

				case MessageProgressType.OnScene:
					progress = message.ProgressUpdates.FirstOrDefault(i => i.ProgressType == MessageProgressType.OnRoute);
					break;

				case MessageProgressType.JobClear:
					progress = message.ProgressUpdates.FirstOrDefault(i => i.ProgressType == MessageProgressType.OnRoute);
					break;
			}

			// return the progress, or empty string if null
			return (progress != null ? progress.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "");
		}

		/// <summary>
		/// Gets the gps coordinates for the export file.
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		private static string GetGpsCoordinates(LocationInfo location)
		{
			if (location == null || location.Coordinates == null)
			{
				return "";
			}
			return location.Coordinates.ToString();
		}

		/// <summary>
		/// Gets the map reference for the export file.
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		private static string GetMapReference(LocationInfo location)
		{
			// If the location info is null, return empty string
			return (location != null ? location.MapReference : "");
		}


	}
}
