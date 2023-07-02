using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.Utility;

namespace JTacticalSim.API.Component
{
	public interface IBoardComponent : IPathableObject, IBaseComponent, IComparable<IBoardComponent>
	{
		ICountry Country { get; set; }

		bool LocationEquals(ICoordinate lhs);
		bool IsFriendly();

	}
}
