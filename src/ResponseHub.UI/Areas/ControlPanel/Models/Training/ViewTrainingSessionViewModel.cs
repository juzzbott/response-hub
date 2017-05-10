using Enivate.ResponseHub.Model.Training;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enivate.ResponseHub.UI.Areas.ControlPanel.Models.Training
{
	public class ViewTrainingSessionViewModel
	{

		public Guid Id { get; set; }

		public string Name { get; set; }

		public DateTime SessionDate { get; set; }

		public IList<TrainingType> TrainingTypes { get; set; }

		public IList<Tuple<Guid, string, string>> Members { get; set; }

		public IList<Tuple<Guid, string, string>> Trainers { get; set; }

		public string Description { get; set; }

		public TrainingSessionType SessionType { get; set; }

		public DateTime Created { get; set; }

		public int MemberPercentTrained { get; set; }

		public decimal Duration { get; set; }

		public int MemberCount
		{
			get
			{
				return Members.Union(Trainers).Distinct().Count();
			}
		}

		public ViewTrainingSessionViewModel()
		{
			Members = new List<Tuple<Guid, string, string>>();
			Trainers = new List<Tuple<Guid, string, string>>();
			TrainingTypes = new List<TrainingType>();
		}

	}
}