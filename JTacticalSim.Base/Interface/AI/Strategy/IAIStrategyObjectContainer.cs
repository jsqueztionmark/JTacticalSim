using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTacticalSim.API.Component;

namespace JTacticalSim.API.AI
{
	public interface IAIStrategyObjectContainer<TChild> : IBaseComponent
	{
		IResult<TChild, TChild> AddChildComponent(TChild component);
		IResult<TChild, TChild> AddChildComponents(IEnumerable<TChild> components);
		IResult<TChild, TChild> RemoveChildComponent(TChild component);
		List<TChild> GetChildComponents();
	}
}
