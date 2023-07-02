using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface IUnitStack : IBoardComponent, IClickableComponent
	{
		int DisplayOrder { get; set; }
		bool HasVisibleComponents { get; }

		IUnit GetTopUnit();
		IUnit GetFirstVisibleUnit();
		List<IUnit> GetAllUnits();
		IResult<IUnit, IUnit> AddUnit(IUnit component);
		IResult<IUnit, IUnit> RemoveUnit(IUnit component);
		IResult<IUnit, IUnit> ClearUnits();

		/// <summary>
		/// Cycles through the stack of units bringing the next to the top of the stack
		/// </summary>
		void CycleUnits();

		void BringToTop(IUnit unit);

		int Render();
	}
}
