using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API;

namespace JTacticalSim.API.InfoObjects
{
	public class StrategicAssessmentInfo
	{
		public StrategicAssessmentRating DefensibleRating { get; set; }
		public StrategicAssessmentRating OffensibleRating { get; set; }
		public StrategicAssessmentRating StealthRating { get; set; }
		public StrategicAssessmentRating MovementRating { get; set; }
		public StrategicAssessmentRating OtherAggragateRating { get; set; }
		public StrategicAssessmentRating VictoryPointsRating { get; set; }

		public StrategicAssessmentInfo()
		{
			DefensibleRating = StrategicAssessmentRating.NONE;
			OffensibleRating = StrategicAssessmentRating.NONE;
			StealthRating = StrategicAssessmentRating.NONE;
			MovementRating = StrategicAssessmentRating.NONE;
			OtherAggragateRating = StrategicAssessmentRating.NONE;
			VictoryPointsRating = StrategicAssessmentRating.NONE;
		}
	}
}
