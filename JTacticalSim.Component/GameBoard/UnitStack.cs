using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.Component;
using JTacticalSim.Utility;
using Extension = JTacticalSim.API.Component.Extension;

namespace JTacticalSim.Component.GameBoard
{
	[Serializable]
	public class UnitStack : BoardComponentBase, IUnitStack
	{
		// Events
		public event ComponentClickedEvent ComponentClicked;
		public int DisplayOrder { get; set; }		
		
		private List<IUnit> Units { get; set; }

		public bool HasVisibleComponents { get { return Units.Any(c => c.IsVisible() && !c.IsHiddenFromEnemy()); }}

		// ----------------------------------------------------------------------------------------------------------

		// Graphics

		public UnitStack(ICountry country, ICoordinate location)
		{
			Units = new List<IUnit>();
			Country = country;
			Location =  location;
		}

		public int Render()
		{
			return TheGame().Renderer.RenderUnitStackInfo(this);
		}

		// ----------------------------------------------------------------------------------------------------------

		public IUnit GetTopUnit() 
		{
			return (Units.OrderBy(u => u.StackOrder).Any()) ? Units.First() : null; 
		}

		public IUnit GetFirstVisibleUnit() 
		{
			return (Units.OrderBy(u => u.StackOrder).Any()) ? Units.First(u => u.IsVisible() && !u.IsHiddenFromEnemy()) : null; 
		}

		public List<IUnit> GetAllUnits() 
		{ 
			return Units;
		}
		
		public IResult<IUnit, IUnit> AddUnit(IUnit unit)
		{
			var r = new OperationResult<IUnit, IUnit> {Status = API.ResultStatus.SUCCESS, Result = unit};

			// Check to be sure the unit is the correct country
			if (!unit.Country.Equals(Country))
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Component must be the same country as the stack.");
				return r;
			}

			try
			{
				Units.Add(unit);
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.FailedObjects = new List<IUnit> {unit};
				return r;
			}

			r.SuccessfulObjects = new List<IUnit> {unit};
			return r;
		}

		public IResult<IUnit, IUnit> RemoveUnit(IUnit unit)
		{
			var r = new OperationResult<IUnit, IUnit> {Status = API.ResultStatus.SUCCESS, Result = unit};

			// Check to be sure the unit is the correct country
			if (!unit.Country.Equals(Country))
			{
				r.Status = ResultStatus.FAILURE;
				r.Messages.Add("Component must be the same country as the stack.");
				return r;
			}

			try
			{
				Units.Remove(unit);
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				r.FailedObjects = new List<IUnit> {unit};
				return r;
			}

			r.SuccessfulObjects = new List<IUnit> {unit};
			return r;
		}

		public IResult<IUnit, IUnit> ClearUnits()
		{
			var r = new OperationResult<IUnit, IUnit> {Status = ResultStatus.SUCCESS};

			try
			{
				Units.Clear();
				// Update and refresh data
				GetNode().ResetUnitStackOrder();
			}
			catch (Exception ex)
			{
				r.Status = ResultStatus.EXCEPTION;
				r.ex = ex;
				return r;
			}

			return r;
		}

		public void CycleUnits()
		{
			// None or one... no need to scroll
			if (Units.Count(u => u.IsVisible()) < 2) return;

			var tmp = GetAllUnits();
			var toBottom = tmp.First();
			var sorted = tmp.Skip(1).ToList();

			Units.Clear();
			sorted.ForEach(u => AddUnit(u));
			AddUnit(toBottom);

			if (!GetTopUnit().IsVisible()) CycleUnits();

			// Update and refresh data
			GetNode().ResetUnitStackOrder();
		}

		public void BringToTop(IUnit unit)
		{
			while (!GetTopUnit().Equals(unit))
			{
				CycleUnits();
			};
		}

		// Event Handlers
		public void On_ComponentClicked(object sender, ComponentClickedEventArgs e)
		{
			MouseButton clicked = e.ButtonClicked;

			// Handle operations based on mouse click
			switch (clicked)
			{
				case MouseButton.DOUBLELEFT :
					{
						this.CycleUnits();
						break;
					}
				case MouseButton.LEFT :
				case MouseButton.MIDDLE :
				case MouseButton.RIGHT :
				case MouseButton.DOUBLERIGHT :
					{
						break;	
					}
					
				default :
					break;
			}

			// Set the clicked unit to the currently selected unit
			this.GetFirstVisibleUnit().Select();

			if (ComponentClicked != null) ComponentClicked(this, e);
		}
	}
}
