using System;
using System.Text;

namespace JTacticalSim.API.Component
{
	public interface IExecutableTask<TObject, TComponent>
	{
		event EventHandler Executing;
		event EventHandler Executed;

		IResult<TObject, TComponent> Execute();
	}
}
