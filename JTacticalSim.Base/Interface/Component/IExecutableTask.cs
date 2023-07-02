using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTacticalSim.API.Component
{
	public interface IExecutableTask<TObject, TComponent>
	{
		event EventHandler Executing;
		event EventHandler Executed;

		IResult<TObject, TComponent> Execute();
	}
}
