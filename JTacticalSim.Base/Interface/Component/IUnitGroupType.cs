using System;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface IUnitGroupType : ITextDisplayable
	{
		int Level { get; set; }
		int MaxDirectAssignedUnits { get; set; }

		IUnitGroupType NextHighestGroupType { get; }
	}
}
