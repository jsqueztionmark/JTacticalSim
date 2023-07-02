using System;
using System.Collections.Generic;

namespace JTacticalSim.API.Component
{
	public interface IResult<TResult, TObject> : IBaseResult
	{
		TResult Result { get; set; }

		List<TObject> FailedObjects { get; set; }
		List<TObject> SuccessfulObjects { get; set; }
	}
}
