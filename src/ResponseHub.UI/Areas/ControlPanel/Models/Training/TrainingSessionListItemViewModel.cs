using Enivate.ResponseHub.Model.Training;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Training
{
	public class TrainingSessionListItemViewModel
	{

		public Guid Id { get; set; }

		public IList<TrainingType> TrainingTypes { get; set; }

		public IList<Guid> Members { get; set; }

		public IList<Guid> Trainers { get; set; }

		public string Name { get; set; }

		public DateTime SessionDate { get; set; }

		public decimal Duration { get; set; }

		public TrainingSessionType SessionType { get; set; }

		public int MemberCount
		{
			get
			{
				return Members.Union(Trainers).Distinct().Count();
			}
		}

		public TrainingSessionListItemViewModel()
		{
			Members = new List<Guid>();
			Trainers = new List<Guid>();
			TrainingTypes = new List<TrainingType>();
		}

		public static TrainingSessionListItemViewModel FromTrainingSession(TrainingSession trainingSession)
		{
			// If the training session is null, return null
			if (trainingSession == null)
			{
				return null;
			}

			return new TrainingSessionListItemViewModel()
			{
				Id = trainingSession.Id,
				Members = trainingSession.Members,
				Trainers = trainingSession.Trainers,
				TrainingTypes = trainingSession.TrainingTypes,
				Name = trainingSession.Name,
				SessionDate = trainingSession.SessionDate,
				Duration = trainingSession.Duration,
				SessionType = trainingSession.SessionType
			};

		}

	}
}